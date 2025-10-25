using BookingService.DTOs;
using BookingService.Models;
using BookingService.Models.ModelSettings;
using BookingService.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text;
using File = System.IO.File;

namespace BookingService.Services
{
    public class OnlineContractService : IOnlineContractService
    {
        private readonly IPdfConverterService _pdfConverter;
        private readonly IAwsS3Service _s3Service; // ✅ Đổi từ ICloudinaryService
        private readonly ContractSettings _contractSettings;
        private readonly PdfSettings _pdfSettings;
        private readonly IOnlineContractRepository _contractRepo;
        private readonly IEmailService _emailService;
        private readonly ILogger<OnlineContractService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        private static readonly SemaphoreSlim _contractNumberLock = new(1, 1);

        public OnlineContractService(
            IAwsS3Service s3Service, // ✅ Đổi từ ICloudinaryService
            IOnlineContractRepository contractRepo,
            IEmailService emailService,
            ILogger<OnlineContractService> logger,
            IUnitOfWork unitOfWork,
            IPdfConverterService pdfConverter,
            IOptions<ContractSettings> contractSettings,
            IOptions<PdfSettings> pdfSettings)
        {
            _s3Service = s3Service ?? throw new ArgumentNullException(nameof(s3Service));
            _contractRepo = contractRepo ?? throw new ArgumentNullException(nameof(contractRepo));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _pdfConverter = pdfConverter ?? throw new ArgumentNullException(nameof(pdfConverter));
            _contractSettings = contractSettings?.Value ?? throw new ArgumentNullException(nameof(contractSettings));
            _pdfSettings = pdfSettings?.Value ?? throw new ArgumentNullException(nameof(pdfSettings));
        }

        /// <summary>
        /// ⭐ METHOD CHÍNH - Tạo hợp đồng từ ContractDataDto với retry mechanism
        /// </summary>
        public async Task<ContractDetailsDto> CreateContractFromDataAsync(ContractDataDto contractData)
        {
            _logger.LogInformation(
                "📝 Creating contract for Order {OrderId}, Customer {CustomerName}",
                contractData.OrderId, contractData.CustomerName);

            const int maxRetries = 3;
            int attempt = 0;

            while (attempt < maxRetries)
            {
                try
                {
                    attempt++;
                    _logger.LogInformation("🔄 Attempt {Attempt}/{MaxRetries} for Order {OrderId}",
                        attempt, maxRetries, contractData.OrderId);

                    // 1. Validate input
                    ValidateContractData(contractData);

                    // 2. Generate unique contract number
                    var contractNumber = await GenerateUniqueContractNumberAsync();
                    contractData.ContractNumber = contractNumber;

                    _logger.LogInformation("✅ Contract number: {ContractNumber}", contractNumber);

                    // 3. Fill default company info
                    FillDefaultCompanyInfo(contractData);

                    // 4. Generate PDF
                    var pdfPath = await GeneratePdfAsync(contractData);
                    _logger.LogInformation("📄 PDF generated: {PdfPath}", pdfPath);

                    // 5. Upload to AWS S3
                    var s3Url = await UploadToS3Async(contractNumber, pdfPath);
                    _logger.LogInformation("☁️ Uploaded to S3: {S3Url}", s3Url);

                    // 6. Create entity
                    var contract = MapToEntity(contractData, s3Url);

                    // 7. Save to database
                    var savedContract = await _contractRepo.CreateAsync(contract);

                    _logger.LogInformation(
                        "💾 Contract {ContractId} saved with number {ContractNumber}",
                        savedContract.OnlineContractId, savedContract.ContractNumber);

                    // 8. Send email asynchronously (fire-and-forget)
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await SendContractEmailAsync(contractData, s3Url);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex,
                                "📧 Failed to send email for Order {OrderId}",
                                contractData.OrderId);
                        }
                    });

                    // 9. Cleanup local file
                    DeleteLocalFile(pdfPath);

                    // 10. Return response
                    return MapToDetailsDto(savedContract);
                }
                catch (DbUpdateException ex) when (IsDuplicateKeyError(ex))
                {
                    if (attempt >= maxRetries)
                    {
                        _logger.LogError(ex,
                            "❌ Failed after {MaxRetries} attempts for Order {OrderId}",
                            maxRetries, contractData.OrderId);
                        throw new InvalidOperationException(
                            $"Không thể tạo hợp đồng sau {maxRetries} lần thử. Vui lòng thử lại sau.");
                    }

                    var delayMs = 100 * (int)Math.Pow(2, attempt - 1);
                    _logger.LogWarning(
                        "⚠️ Duplicate key on attempt {Attempt}. Retrying after {DelayMs}ms...",
                        attempt, delayMs);
                    await Task.Delay(delayMs);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error creating contract for Order {OrderId}", contractData.OrderId);
                    throw new InvalidOperationException(
                        $"Không thể tạo hợp đồng cho đơn hàng #{contractData.OrderId}", ex);
                }
            }

            throw new InvalidOperationException($"Không thể tạo hợp đồng sau {maxRetries} lần thử.");
        }

        #region Validation

        private void ValidateContractData(ContractDataDto data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var errors = new List<string>();

            if (data.OrderId <= 0) errors.Add("OrderId phải > 0");
            if (string.IsNullOrWhiteSpace(data.CustomerName)) errors.Add("Tên khách hàng trống");
            if (string.IsNullOrWhiteSpace(data.CustomerEmail)) errors.Add("Email trống");
            if (string.IsNullOrWhiteSpace(data.CustomerPhone)) errors.Add("SĐT trống");
            if (string.IsNullOrWhiteSpace(data.CustomerIdCard)) errors.Add("CMND/CCCD trống");
            if (string.IsNullOrWhiteSpace(data.VehicleModel)) errors.Add("Model xe trống");
            if (string.IsNullOrWhiteSpace(data.LicensePlate)) errors.Add("Biển số trống");
            if (data.FromDate >= data.ToDate) errors.Add("Ngày bắt đầu >= ngày kết thúc");
            if (data.TotalRentalCost <= 0) errors.Add("Chi phí thuê <= 0");
            if (data.TotalPaymentAmount <= 0) errors.Add("Tổng thanh toán <= 0");
            if (string.IsNullOrWhiteSpace(data.TransactionId)) errors.Add("Mã giao dịch trống");

            if (errors.Any())
            {
                var errorMessage = string.Join("; ", errors);
                _logger.LogWarning("⚠️ Validation failed for Order {OrderId}: {Errors}",
                    data.OrderId, errorMessage);
                throw new InvalidOperationException($"Dữ liệu không hợp lệ: {errorMessage}");
            }

            _logger.LogInformation("✅ Validation passed for Order {OrderId}", data.OrderId);
        }

        #endregion

        #region Contract Number Generation

        private async Task<string> GenerateUniqueContractNumberAsync()
        {
            await _contractNumberLock.WaitAsync();

            try
            {
                var prefix = _contractSettings.NumberPrefix;
                var dateFormat = _contractSettings.DateFormat;
                var today = DateTime.UtcNow.ToString(dateFormat);
                var datePrefix = $"{prefix}-{today}-";

                var latestContractNumber = await _contractRepo.GetLatestContractNumberByDateAsync(datePrefix);

                int nextNumber = 1;

                if (!string.IsNullOrEmpty(latestContractNumber))
                {
                    var numberPart = latestContractNumber.Substring(datePrefix.Length);
                    if (int.TryParse(numberPart, out int currentNumber))
                    {
                        nextNumber = currentNumber + 1;
                    }
                }

                var padding = _contractSettings.OrderIdPadding;
                var contractNumber = $"{datePrefix}{nextNumber.ToString($"D{padding}")}";

                // Double check
                var exists = await _contractRepo.ExistsByContractNumberAsync(contractNumber);
                if (exists)
                {
                    nextNumber++;
                    contractNumber = $"{datePrefix}{nextNumber.ToString($"D{padding}")}";
                    _logger.LogWarning("⚠️ Collision detected, incremented to: {ContractNumber}", contractNumber);
                }

                return contractNumber;
            }
            finally
            {
                _contractNumberLock.Release();
            }
        }

        #endregion

        #region PDF Generation

        public async Task<string> GeneratePdfAsync(ContractDataDto contractData)
        {
            _logger.LogInformation("📄 Generating PDF for Order {OrderId}", contractData.OrderId);

            try
            {
                var htmlContent = BuildContractHtml(contractData);
                var fileName = $"Contract_{contractData.OrderId}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
                var contractsDir = _contractSettings.StoragePath;
                var fullPath = Path.Combine(contractsDir, fileName);

                Directory.CreateDirectory(contractsDir);

                await ConvertHtmlToPdfAsync(htmlContent, fullPath);

                _logger.LogInformation("✅ PDF generated at {FilePath}", fullPath);
                return fullPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ PDF generation failed for Order {OrderId}", contractData.OrderId);
                throw new InvalidOperationException("Không thể tạo file PDF hợp đồng", ex);
            }
        }

        private async Task ConvertHtmlToPdfAsync(string htmlContent, string outputPath)
        {
            try
            {
                _logger.LogInformation("🔄 Converting HTML to PDF: {OutputPath}", outputPath);

                await _pdfConverter.ConvertHtmlToPdfAsync(htmlContent, outputPath);

                if (_contractSettings.SaveDebugHtml)
                {
                    var htmlPath = outputPath.Replace(".pdf", ".html");
                    await File.WriteAllTextAsync(htmlPath, htmlContent);
                    _logger.LogDebug("🐛 Debug HTML saved: {HtmlPath}", htmlPath);
                }

                _logger.LogInformation("✅ PDF conversion completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ HTML to PDF conversion failed");
                throw new InvalidOperationException($"Không thể tạo PDF: {ex.Message}", ex);
            }
        }

        #endregion

        #region AWS S3 Upload

        /// <summary>
        /// ✅ Upload PDF to AWS S3 and return public URL
        /// </summary>
        private async Task<string> UploadToS3Async(string contractNumber, string pdfPath)
        {
            try
            {
                _logger.LogInformation("☁️ Uploading contract {ContractNumber} to S3", contractNumber);

                using var fileStream = File.OpenRead(pdfPath);
                var fileName = $"{contractNumber}.pdf";

                var s3Url = await _s3Service.UploadFileAsync(fileStream, fileName, "application/pdf");

                if (string.IsNullOrEmpty(s3Url))
                {
                    throw new InvalidOperationException("S3 upload returned empty URL");
                }

                _logger.LogInformation("✅ S3 upload successful: {Url}", s3Url);
                return s3Url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ S3 upload failed for contract {ContractNumber}", contractNumber);
                throw new InvalidOperationException($"Không thể upload file lên S3: {ex.Message}", ex);
            }
        }

        #endregion

        #region Email

        private async Task SendContractEmailAsync(ContractDataDto data, string s3Url)
        {
            try
            {
                _logger.LogInformation(
                    "📧 Sending email to {Email} for Order {OrderId}",
                    data.CustomerEmail, data.OrderId);

                var emailSent = await _emailService.SendContractEmailAsync(
                    toEmail: data.CustomerEmail,
                    customerName: data.CustomerName,
                    contractNumber: data.ContractNumber,
                    driveLink: s3Url); // Tên parameter giữ nguyên để tương thích

                if (emailSent)
                {
                    _logger.LogInformation(
                        "✅ Email sent to {Email} for Order {OrderId}",
                        data.CustomerEmail, data.OrderId);
                }
                else
                {
                    _logger.LogWarning(
                        "⚠️ Email send failed to {Email} for Order {OrderId}",
                        data.CustomerEmail, data.OrderId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ Email error for {Email}, Order {OrderId}",
                    data.CustomerEmail, data.OrderId);
            }
        }

        #endregion

        #region Mapping

        private OnlineContract MapToEntity(ContractDataDto data, string s3Url)
        {
            return new OnlineContract
            {
                OrderId = data.OrderId,
                ContractNumber = data.ContractNumber,
                ContractFilePath = s3Url, // ✅ S3 URL thay vì local path
                SignedAt = data.PaidAt ?? data.PaymentDate,
                SignatureData = data.TransactionId,
                TemplateVersion = 1,
                CreatedAt = DateTime.UtcNow
            };
        }

        private ContractDetailsDto MapToDetailsDto(OnlineContract contract)
        {
            return new ContractDetailsDto
            {
                ContractId = contract.OnlineContractId,
                OrderId = contract.OrderId,
                ContractNumber = contract.ContractNumber,
                Status = ContractStatus.Signed,
                DownloadUrl = contract.ContractFilePath, // ✅ S3 URL
                CreatedAt = contract.CreatedAt,
                PaidAt = contract.SignedAt,
                TransactionId = contract.SignatureData,
                Message = "Hợp đồng đã được tạo và gửi qua email thành công."
            };
        }

        #endregion

        #region HTML Generation

        private string BuildContractHtml(ContractDataDto data)
        {
            var sb = new StringBuilder();

            AppendHtmlHeader(sb);
            AppendContractHeader(sb, data);
            AppendCompanyInfo(sb, data);
            AppendCustomerInfo(sb, data);
            AppendVehicleInfo(sb, data);
            AppendRentalInfo(sb, data);
            AppendFinancialInfo(sb, data);
            AppendPaymentInfo(sb, data);
            AppendTermsAndConditions(sb);
            AppendSignatures(sb, data);

            sb.AppendLine("</body></html>");

            return sb.ToString();
        }

        private void AppendHtmlHeader(StringBuilder sb)
        {
            sb.AppendLine(@"<!DOCTYPE html>
            <html lang='vi'>
            <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>Hợp Đồng Thuê Xe</title>
            <style>
            body { font-family: 'Times New Roman', serif; margin: 40px; line-height: 1.6; color: #333; }
            .header { text-align: center; margin-bottom: 30px; border-bottom: 3px solid #333; padding-bottom: 20px; }
            h1 { text-transform: uppercase; margin: 10px 0; font-size: 24px; }
            h2 { color: #2c3e50; margin-top: 25px; margin-bottom: 15px; font-size: 18px; border-bottom: 2px solid #3498db; padding-bottom: 5px; }
            table { width: 100%; border-collapse: collapse; margin: 15px 0; }
            th, td { border: 1px solid #ddd; padding: 10px; text-align: left; }
            th { background-color: #3498db; color: white; font-weight: bold; }
            .info-row td:first-child { font-weight: bold; width: 35%; background-color: #f8f9fa; }
            .total-row { background-color: #fff3cd; font-weight: bold; font-size: 16px; }
            .section { margin: 25px 0; page-break-inside: avoid; }
            .signatures { margin-top: 50px; display: table; width: 100%; }
            .signature-box { display: table-cell; width: 50%; text-align: center; padding: 20px; }
            .signature-line { margin-top: 80px; border-top: 1px solid #333; padding-top: 5px; display: inline-block; min-width: 200px; }
            .terms { background-color: #f8f9fa; padding: 15px; border-left: 4px solid #3498db; margin: 20px 0; }
            .terms ul { margin: 10px 0; padding-left: 20px; }
            .terms li { margin: 8px 0; }
            </style>
            </head>
            <body>");
        }

        private void AppendContractHeader(StringBuilder sb, ContractDataDto data)
        {
            sb.AppendLine($@"
            <div class='header'>
            <h1>HỢP ĐỒNG THUÊ XE TỰ LÁI</h1>
            <p><strong>Số hợp đồng: {data.ContractNumber}</strong></p>
            <p>Ngày tạo: {DateTime.UtcNow:dd/MM/yyyy HH:mm}</p>
            </div>");
        }

        private void AppendCompanyInfo(StringBuilder sb, ContractDataDto data)
        {
            sb.AppendLine($@"
            <div class='section'>
            <h2>BÊN CHO THUÊ (BÊN A)</h2>
            <table>
            <tr class='info-row'><td>Tên công ty:</td><td>{data.CompanyName}</td></tr>
            <tr class='info-row'><td>Địa chỉ:</td><td>{data.CompanyAddress}</td></tr>
            <tr class='info-row'><td>Mã số thuế:</td><td>{data.CompanyTaxCode}</td></tr>
            <tr class='info-row'><td>Người đại diện:</td><td>{data.CompanyRepresentative}</td></tr>
            </table>
            </div>");
        }

        private void AppendCustomerInfo(StringBuilder sb, ContractDataDto data)
        {
            sb.AppendLine($@"
            <div class='section'>
            <h2>BÊN THUÊ (BÊN B)</h2>
            <table>
            <tr class='info-row'><td>Họ và tên:</td><td>{data.CustomerName}</td></tr>
            <tr class='info-row'><td>Ngày sinh:</td><td>{data.CustomerDateOfBirth}</td></tr>
            <tr class='info-row'><td>CMND/CCCD:</td><td>{data.CustomerIdCard}</td></tr>
            <tr class='info-row'><td>Địa chỉ:</td><td>{data.CustomerAddress}</td></tr>
            <tr class='info-row'><td>Điện thoại:</td><td>{data.CustomerPhone}</td></tr>
            <tr class='info-row'><td>Email:</td><td>{data.CustomerEmail}</td></tr>
            </table>
            </div>");
        }

        private void AppendVehicleInfo(StringBuilder sb, ContractDataDto data)
        {
            sb.AppendLine($@"
            <div class='section'>
            <h2>THÔNG TIN XE</h2>
            <table>
            <tr class='info-row'><td>Model:</td><td>{data.VehicleModel}</td></tr>
            <tr class='info-row'><td>Biển kiểm soát:</td><td>{data.LicensePlate}</td></tr>
            <tr class='info-row'><td>Loại xe:</td><td>{data.VehicleType}</td></tr>
            <tr class='info-row'><td>Màu sắc:</td><td>{data.VehicleColor}</td></tr>
            </table>
            </div>");
        }

        private void AppendRentalInfo(StringBuilder sb, ContractDataDto data)
        {
            var duration = data.ToDate - data.FromDate;
            sb.AppendLine($@"
            <div class='section'>
            <h2>THỜI GIAN THUÊ XE</h2>
            <table>
            <tr class='info-row'><td>Ngày giờ nhận xe:</td><td>{data.FromDate:dd/MM/yyyy HH:mm}</td></tr>
            <tr class='info-row'><td>Ngày giờ trả xe:</td><td>{data.ToDate:dd/MM/yyyy HH:mm}</td></tr>
            <tr class='info-row'><td>Tổng thời gian:</td><td>{(int)duration.TotalDays} ngày {duration.Hours} giờ</td></tr>
            </table>
            </div>");
        }

        private void AppendFinancialInfo(StringBuilder sb, ContractDataDto data)
        {
            sb.AppendLine($@"
            <div class='section'>
            <h2>THÔNG TIN TÀI CHÍNH</h2>
            <table>
            <tr class='info-row'><td>Phí thuê xe:</td><td>{data.TotalRentalCost:N0} VNĐ</td></tr>
            <tr class='info-row'><td>Tiền đặt cọc (30%):</td><td>{data.DepositAmount:N0} VNĐ</td></tr>
            <tr class='info-row'><td>Phí dịch vụ:</td><td>{data.ServiceFee:N0} VNĐ</td></tr>
            <tr class='total-row'><td>TỔNG THANH TOÁN:</td><td>{data.TotalPaymentAmount:N0} VNĐ</td></tr>
            </table>
            </div>");
        }

        private void AppendPaymentInfo(StringBuilder sb, ContractDataDto data)
        {
            sb.AppendLine($@"
            <div class='section'>
            <h2>THÔNG TIN THANH TOÁN</h2>
            <table>
            <tr class='info-row'><td>Mã giao dịch:</td><td>{data.TransactionId}</td></tr>
            <tr class='info-row'><td>Phương thức:</td><td>{data.PaymentMethod}</td></tr>
            <tr class='info-row'><td>Ngày thanh toán:</td><td>{data.PaymentDate:dd/MM/yyyy HH:mm}</td></tr>
            <tr class='info-row'><td>Trạng thái:</td><td><strong style='color: green;'>ĐÃ THANH TOÁN</strong></td></tr>
            </table>
            </div>");
        }

        private void AppendTermsAndConditions(StringBuilder sb)
        {
            sb.AppendLine(@"
            <div class='section'>
            <h2>ĐIỀU KHOẢN VÀ ĐIỀU KIỆN</h2>
            <div class='terms'>
            <ul>
            <li>Bên B cam kết sử dụng xe đúng mục đích, giữ gìn xe như tài sản của mình.</li>
            <li>Bên B chịu trách nhiệm về các vi phạm giao thông trong thời gian thuê xe.</li>
            <li>Bên B phải hoàn trả xe đúng địa điểm và thời gian quy định trong hợp đồng.</li>
            <li>Tiền đặt cọc sẽ được hoàn trả sau khi kiểm tra xe không có hư hỏng.</li>
            <li>Trường hợp xe bị hư hỏng do lỗi của Bên B, chi phí sửa chữa sẽ do Bên B chịu.</li>
            <li>Bên B không được cho thuê lại xe cho bên thứ ba khi chưa có sự đồng ý của Bên A.</li>
            <li>Mọi tranh chấp phát sinh sẽ được giải quyết thông qua thương lượng hoặc theo pháp luật Việt Nam.</li>
            </ul>
            </div>
            </div>");
        }

        private void AppendSignatures(StringBuilder sb, ContractDataDto data)
        {
            sb.AppendLine($@"
            <div class='signatures'>
            <div class='signature-box'>
            <p><strong>BÊN CHO THUÊ</strong></p>
            <p>{data.CompanyRepresentative}</p>
            <div class='signature-line'>Chữ ký</div>
            </div>
            <div class='signature-box'>
            <p><strong>BÊN THUÊ</strong></p>
            <p>{data.CustomerName}</p>
            <div class='signature-line'>Chữ ký điện tử</div>
            <p style='font-size: 12px; color: #666; margin-top: 10px;'>Đã ký điện tử lúc {data.PaymentDate:HH:mm dd/MM/yyyy}</p>
            </div>
            </div>");
        }

        #endregion

        #region Utilities

        private void FillDefaultCompanyInfo(ContractDataDto data)
        {
            data.CompanyName ??= _contractSettings.CompanyName;
            data.CompanyAddress ??= _contractSettings.CompanyAddress;
            data.CompanyTaxCode ??= _contractSettings.CompanyTaxCode;
            data.CompanyRepresentative ??= _contractSettings.CompanyRepresentative;
        }

        private void DeleteLocalFile(string pdfPath)
        {
            try
            {
                if (File.Exists(pdfPath))
                {
                    File.Delete(pdfPath);
                    _logger.LogInformation("🗑️ Deleted local file: {PdfPath}", pdfPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Failed to delete local file: {PdfPath}", pdfPath);
            }
        }

        private static bool IsDuplicateKeyError(DbUpdateException ex)
        {
            return ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx && sqlEx.Number == 2601;
        }

        #endregion
    }
}