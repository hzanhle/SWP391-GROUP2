using BookingService.DTOs;
using BookingService.Models;
using BookingService.Repositories;
using Microsoft.Extensions.Options;
using System.Text;

namespace BookingService.Services
{
    public class OnlineContractService : IOnlineContractService
    {
        private readonly IPdfConverterService _pdfConverter;
        private readonly ContractSettings _contractSettings;
        private readonly PdfSettings _pdfSettings;
        private readonly IOnlineContractRepository _contractRepo;
        private readonly IEmailService _emailService;
        private readonly ILogger<OnlineContractService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public OnlineContractService(
            IOnlineContractRepository contractRepo,
            IEmailService emailService,
            ILogger<OnlineContractService> logger,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            IPdfConverterService pdfConverter,
            IOptions<ContractSettings> contractSettings,
            IOptions<PdfSettings> pdfSettings)
        {
            _contractRepo = contractRepo;
            _emailService = emailService;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _pdfConverter = pdfConverter;
            _contractSettings = contractSettings?.Value ?? throw new ArgumentNullException(nameof(contractSettings));
            _pdfSettings = pdfSettings?.Value ?? throw new ArgumentNullException(nameof(pdfSettings));
        }

        /// <summary>
        /// ⭐ METHOD CHÍNH - Tạo hợp đồng từ ContractDataDto (được Frontend gửi lên)
        /// </summary>
        public async Task<ContractDetailsDto> CreateContractFromDataAsync(ContractDataDto contractData)
        {
            _logger.LogInformation(
                "Creating contract from data for Order {OrderId}, Customer {CustomerName}",
                contractData.OrderId, contractData.CustomerName);

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // 1. Validate dữ liệu đầu vào
                ValidateContractData(contractData);

                // 2. Tạo contract number duy nhất
                var contractNumber = GenerateContractNumber(contractData.OrderId);
                contractData.ContractNumber = contractNumber;

                // 2.1. Fill default company info nếu chưa có
                FillDefaultCompanyInfo(contractData);

                // 3. Tạo PDF từ dữ liệu
                var pdfPath = await GeneratePdfAsync(contractData);
                _logger.LogInformation("PDF generated at {PdfPath}", pdfPath);

                // 4. Tạo entity OnlineContract
                var contract = MapToEntity(contractData, pdfPath);

                // 5. Lưu vào database
                var savedContract = await _contractRepo.CreateAsync(contract);

                _logger.LogInformation(
                    "Contract {ContractId} saved to database with number {ContractNumber}",
                    savedContract.OnlineContractId, savedContract.ContractNumber);

                // 6. Commit transaction (SaveChanges tự động)
                await _unitOfWork.CommitTransactionAsync();

                // 7. Gửi email bất đồng bộ SAU KHI COMMIT (không block response)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await SendContractEmailAsync(contractData, pdfPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Failed to send contract email for Order {OrderId}",
                            contractData.OrderId);
                        // Không throw - email fail không ảnh hưởng contract creation
                    }
                });

                // 8. Trả về response
                return MapToDetailsDto(savedContract);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contract for Order {OrderId}", contractData.OrderId);
                await _unitOfWork.RollbackTransactionAsync();
                throw new InvalidOperationException(
                    $"Không thể tạo hợp đồng cho đơn hàng #{contractData.OrderId}", ex);
            }
        }

        #region Private Helpers - Validation

        /// <summary>
        /// ⚠️ ĐÃ FIX: Bỏ check FromDate trong quá khứ
        /// </summary>
        private void ValidateContractData(ContractDataDto data)
        {
            var errors = new List<string>();

            if (data == null)
                throw new ArgumentNullException(nameof(data), "ContractDataDto không được null");

            // Validate Order
            if (data.OrderId <= 0)
                errors.Add("OrderId phải lớn hơn 0");

            // Validate Customer
            if (string.IsNullOrWhiteSpace(data.CustomerName))
                errors.Add("Tên khách hàng không được trống");

            if (string.IsNullOrWhiteSpace(data.CustomerEmail))
                errors.Add("Email khách hàng không được trống");

            if (string.IsNullOrWhiteSpace(data.CustomerPhone))
                errors.Add("Số điện thoại khách hàng không được trống");

            if (string.IsNullOrWhiteSpace(data.CustomerIdCard))
                errors.Add("CMND/CCCD khách hàng không được trống");

            // Validate Vehicle
            if (string.IsNullOrWhiteSpace(data.VehicleModel))
                errors.Add("Model xe không được trống");

            if (string.IsNullOrWhiteSpace(data.LicensePlate))
                errors.Add("Biển số xe không được trống");

            // Validate Dates
            if (data.FromDate >= data.ToDate)
                errors.Add("Ngày bắt đầu phải nhỏ hơn ngày kết thúc");

            // ✅ ĐÃ BỎ CHECK NÀY - User đã thanh toán rồi, không cần validate FromDate nữa
            // ❌ TRƯỚC ĐÂY:
            // if (data.FromDate < DateTime.UtcNow.AddHours(-1))
            //     errors.Add("Ngày bắt đầu không được trong quá khứ");

            // Validate Financial
            if (data.TotalRentalCost <= 0)
                errors.Add("Tổng chi phí thuê phải lớn hơn 0");

            if (data.TotalPaymentAmount <= 0)
                errors.Add("Tổng thanh toán phải lớn hơn 0");

            // Validate Payment
            if (string.IsNullOrWhiteSpace(data.TransactionId))
                errors.Add("Mã giao dịch không được trống");

            if (errors.Any())
            {
                var errorMessage = string.Join("; ", errors);
                _logger.LogWarning("Validation failed for Order {OrderId}: {Errors}",
                    data.OrderId, errorMessage);
                throw new InvalidOperationException($"Dữ liệu hợp đồng không hợp lệ: {errorMessage}");
            }

            _logger.LogInformation("ContractDataDto validated successfully for Order {OrderId}",
                data.OrderId);
        }

        #endregion

        #region Private Helpers - PDF Generation

        /// <summary>
        /// Tạo PDF từ ContractDataDto
        /// </summary>
        public async Task<string> GeneratePdfAsync(ContractDataDto contractData)
        {
            _logger.LogInformation("Generating PDF for Order {OrderId}", contractData.OrderId);

            try
            {
                // 1. Build HTML content
                var htmlContent = BuildContractHtml(contractData);

                // 2. Generate file path
                var fileName = $"Contract_{contractData.OrderId}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
                var contractsDir = _contractSettings.StoragePath;
                var fullPath = Path.Combine(contractsDir, fileName);

                // 3. Ensure directory exists
                Directory.CreateDirectory(contractsDir);

                // 4. Convert HTML to PDF
                await ConvertHtmlToPdfAsync(htmlContent, fullPath);

                _logger.LogInformation("PDF generated successfully at {FilePath}", fullPath);
                return fullPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF for Order {OrderId}", contractData.OrderId);
                throw new InvalidOperationException("Không thể tạo file PDF hợp đồng", ex);
            }
        }

        /// <summary>
        /// Convert HTML to PDF using IPdfConverterService
        /// </summary>
        private async Task ConvertHtmlToPdfAsync(string htmlContent, string outputPath)
        {
            try
            {
                _logger.LogInformation("Converting HTML to PDF: {OutputPath}", outputPath);

                // Sử dụng IPdfConverterService (đã inject PuppeteerPdfService)
                await _pdfConverter.ConvertHtmlToPdfAsync(htmlContent, outputPath);

                // Optional: Save HTML for debugging
                if (_contractSettings.SaveDebugHtml)
                {
                    var htmlPath = outputPath.Replace(".pdf", ".html");
                    await File.WriteAllTextAsync(htmlPath, htmlContent);
                    _logger.LogDebug("Debug HTML saved at {HtmlPath}", htmlPath);
                }

                _logger.LogInformation("PDF conversion completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting HTML to PDF");
                throw new InvalidOperationException($"Không thể tạo PDF: {ex.Message}", ex);
            }
        }

        #endregion

        #region Private Helpers - Mapping

        /// <summary>
        /// Map ContractDataDto to OnlineContract entity
        /// </summary>
        private OnlineContract MapToEntity(ContractDataDto data, string pdfPath)
        {
            return new OnlineContract
            {
                OrderId = data.OrderId,
                ContractNumber = data.ContractNumber,
                ContractFilePath = pdfPath,
                SignedAt = data.PaidAt ?? data.PaymentDate,
                SignatureData = data.TransactionId,
                TemplateVersion = 1,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Map OnlineContract entity to ContractDetailsDto
        /// </summary>
        private ContractDetailsDto MapToDetailsDto(OnlineContract contract)
        {
            return new ContractDetailsDto
            {
                ContractId = contract.OnlineContractId,
                OrderId = contract.OrderId,
                ContractNumber = contract.ContractNumber,
                Status = ContractStatus.Signed, // Đã thanh toán thành công
                DownloadUrl = GenerateDownloadUrl(contract.ContractFilePath),
                CreatedAt = contract.CreatedAt,
                PaidAt = contract.SignedAt,
                TransactionId = contract.SignatureData,
                Message = "Hợp đồng đã được tạo và gửi qua email thành công."
            };
        }

        #endregion

        #region Private Helpers - HTML Generation

        /// <summary>
        /// Xây dựng HTML content cho hợp đồng
        /// </summary>
        private string BuildContractHtml(ContractDataDto data)
        {
            var sb = new StringBuilder();

            // HTML Header
            AppendHtmlHeader(sb);

            // Contract Header
            AppendContractHeader(sb, data);

            // Section I: Company Info
            AppendCompanyInfo(sb, data);

            // Section II: Customer Info
            AppendCustomerInfo(sb, data);

            // Section III: Vehicle Info
            AppendVehicleInfo(sb, data);

            // Section IV: Rental Info
            AppendRentalInfo(sb, data);

            // Section V: Financial Info
            AppendFinancialInfo(sb, data);

            // Section VI: Payment Info
            AppendPaymentInfo(sb, data);

            // Section VII: Terms & Conditions
            AppendTermsAndConditions(sb);

            // Signatures
            AppendSignatures(sb, data);

            // HTML Footer
            sb.AppendLine("</body></html>");

            return sb.ToString();
        }

        private void AppendHtmlHeader(StringBuilder sb)
        {
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang='vi'>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset='UTF-8'>");
            sb.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            sb.AppendLine("<title>Hợp Đồng Thuê Xe</title>");
            sb.AppendLine("<style>");
            sb.AppendLine(@"
                body { 
                    font-family: 'Times New Roman', serif; 
                    margin: 40px; 
                    line-height: 1.6;
                    color: #333;
                }
                .header { 
                    text-align: center; 
                    margin-bottom: 30px;
                    border-bottom: 3px solid #333;
                    padding-bottom: 20px;
                }
                h1 { 
                    text-transform: uppercase;
                    margin: 10px 0;
                    font-size: 24px;
                }
                h2 { 
                    color: #2c3e50;
                    margin-top: 25px;
                    margin-bottom: 15px;
                    font-size: 18px;
                    border-bottom: 2px solid #3498db;
                    padding-bottom: 5px;
                }
                table { 
                    width: 100%; 
                    border-collapse: collapse; 
                    margin: 15px 0;
                }
                th, td { 
                    border: 1px solid #ddd; 
                    padding: 10px; 
                    text-align: left;
                }
                th { 
                    background-color: #3498db;
                    color: white;
                    font-weight: bold;
                }
                .info-row td:first-child {
                    font-weight: bold;
                    width: 35%;
                    background-color: #f8f9fa;
                }
                .total-row {
                    background-color: #fff3cd;
                    font-weight: bold;
                    font-size: 16px;
                }
                .section { 
                    margin: 25px 0;
                    page-break-inside: avoid;
                }
                .signatures {
                    margin-top: 50px;
                    display: table;
                    width: 100%;
                }
                .signature-box {
                    display: table-cell;
                    width: 50%;
                    text-align: center;
                    padding: 20px;
                }
                .signature-line {
                    margin-top: 80px;
                    border-top: 1px solid #333;
                    padding-top: 5px;
                    display: inline-block;
                    min-width: 200px;
                }
                .terms {
                    background-color: #f8f9fa;
                    padding: 15px;
                    border-left: 4px solid #3498db;
                    margin: 20px 0;
                }
                .terms ul {
                    margin: 10px 0;
                    padding-left: 20px;
                }
                .terms li {
                    margin: 8px 0;
                }
            ");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
        }

        private void AppendContractHeader(StringBuilder sb, ContractDataDto data)
        {
            sb.AppendLine("<div class='header'>");
            sb.AppendLine("<h1>HỢP ĐỒNG THUÊ XE TỰ LÁI</h1>");
            sb.AppendLine($"<p><strong>Số hợp đồng: {data.ContractNumber}</strong></p>");
            sb.AppendLine($"<p>Ngày tạo: {DateTime.UtcNow:dd/MM/yyyy HH:mm}</p>");
            sb.AppendLine("</div>");
        }

        private void AppendCompanyInfo(StringBuilder sb, ContractDataDto data)
        {
            sb.AppendLine("<div class='section'>");
            sb.AppendLine("<h2>BÊN CHO THUÊ (BÊN A)</h2>");
            sb.AppendLine("<table>");
            sb.AppendLine($"<tr class='info-row'><td>Tên công ty:</td><td>{data.CompanyName}</td></tr>");
            sb.AppendLine($"<tr class='info-row'><td>Địa chỉ:</td><td>{data.CompanyAddress}</td></tr>");
            sb.AppendLine($"<tr class='info-row'><td>Mã số thuế:</td><td>{data.CompanyTaxCode}</td></tr>");
            sb.AppendLine($"<tr class='info-row'><td>Người đại diện:</td><td>{data.CompanyRepresentative}</td></tr>");
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");
        }

        private void AppendCustomerInfo(StringBuilder sb, ContractDataDto data)
        {
            sb.AppendLine("<div class='section'>");
            sb.AppendLine("<h2>BÊN THUÊ (BÊN B)</h2>");
            sb.AppendLine("<table>");
            sb.AppendLine($"<tr class='info-row'><td>Họ và tên:</td><td>{data.CustomerName}</td></tr>");
            sb.AppendLine($"<tr class='info-row'><td>Ngày sinh:</td><td>{data.CustomerDateOfBirth}</td></tr>");
            sb.AppendLine($"<tr class='info-row'><td>CMND/CCCD:</td><td>{data.CustomerIdCard}</td></tr>");
            sb.AppendLine($"<tr class='info-row'><td>Địa chỉ:</td><td>{data.CustomerAddress}</td></tr>");
            sb.AppendLine($"<tr class='info-row'><td>Điện thoại:</td><td>{data.CustomerPhone}</td></tr>");
            sb.AppendLine($"<tr class='info-row'><td>Email:</td><td>{data.CustomerEmail}</td></tr>");
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");
        }

        private void AppendVehicleInfo(StringBuilder sb, ContractDataDto data)
        {
            sb.AppendLine("<div class='section'>");
            sb.AppendLine("<h2>THÔNG TIN XE</h2>");
            sb.AppendLine("<table>");
            sb.AppendLine($"<tr class='info-row'><td>Model:</td><td>{data.VehicleModel}</td></tr>");
            sb.AppendLine($"<tr class='info-row'><td>Biển kiểm soát:</td><td>{data.LicensePlate}</td></tr>");
            sb.AppendLine($"<tr class='info-row'><td>Loại xe:</td><td>{data.VehicleType}</td></tr>");
            sb.AppendLine($"<tr class='info-row'><td>Màu sắc:</td><td>{data.VehicleColor}</td></tr>");
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");
        }

        private void AppendRentalInfo(StringBuilder sb, ContractDataDto data)
        {
            var duration = data.ToDate - data.FromDate;
            var days = (int)duration.TotalDays;
            var hours = duration.Hours;

            sb.AppendLine("<div class='section'>");
            sb.AppendLine("<h2>THỜI GIAN THUÊ XE</h2>");
            sb.AppendLine("<table>");
            sb.AppendLine($"<tr class='info-row'><td>Ngày giờ nhận xe:</td><td>{data.FromDate:dd/MM/yyyy HH:mm}</td></tr>");
            sb.AppendLine($"<tr class='info-row'><td>Ngày giờ trả xe:</td><td>{data.ToDate:dd/MM/yyyy HH:mm}</td></tr>");
            sb.AppendLine($"<tr class='info-row'><td>Tổng thời gian:</td><td>{days} ngày {hours} giờ</td></tr>");
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");
        }

        private void AppendFinancialInfo(StringBuilder sb, ContractDataDto data)
        {
            sb.AppendLine("<div class='section'>");
            sb.AppendLine("<h2>THÔNG TIN TÀI CHÍNH</h2>");
            sb.AppendLine("<table>");
            sb.AppendLine($"<tr class='info-row'><td>Phí thuê xe:</td><td>{data.TotalRentalCost:N0} VNĐ</td></tr>");
            sb.AppendLine($"<tr class='info-row'><td>Tiền đặt cọc (30%):</td><td>{data.DepositAmount:N0} VNĐ</td></tr>");
            sb.AppendLine($"<tr class='info-row'><td>Phí dịch vụ:</td><td>{data.ServiceFee:N0} VNĐ</td></tr>");
            sb.AppendLine($"<tr class='total-row'><td>TỔNG THANH TOÁN:</td><td>{data.TotalPaymentAmount:N0} VNĐ</td></tr>");
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");
        }

        private void AppendPaymentInfo(StringBuilder sb, ContractDataDto data)
        {
            sb.AppendLine("<div class='section'>");
            sb.AppendLine("<h2>THÔNG TIN THANH TOÁN</h2>");
            sb.AppendLine("<table>");
            sb.AppendLine($"<tr class='info-row'><td>Mã giao dịch:</td><td>{data.TransactionId}</td></tr>");
            sb.AppendLine($"<tr class='info-row'><td>Phương thức:</td><td>{data.PaymentMethod}</td></tr>");
            sb.AppendLine($"<tr class='info-row'><td>Ngày thanh toán:</td><td>{data.PaymentDate:dd/MM/yyyy HH:mm}</td></tr>");
            sb.AppendLine($"<tr class='info-row'><td>Trạng thái:</td><td><strong style='color: green;'>ĐÃ THANH TOÁN</strong></td></tr>");
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");
        }

        private void AppendTermsAndConditions(StringBuilder sb)
        {
            sb.AppendLine("<div class='section'>");
            sb.AppendLine("<h2>ĐIỀU KHOẢN VÀ ĐIỀU KIỆN</h2>");
            sb.AppendLine("<div class='terms'>");
            sb.AppendLine("<ul>");
            sb.AppendLine("<li>Bên B cam kết sử dụng xe đúng mục đích, giữ gìn xe như tài sản của mình.</li>");
            sb.AppendLine("<li>Bên B chịu trách nhiệm về các vi phạm giao thông trong thời gian thuê xe.</li>");
            sb.AppendLine("<li>Bên B phải hoàn trả xe đúng địa điểm và thời gian quy định trong hợp đồng.</li>");
            sb.AppendLine("<li>Tiền đặt cọc sẽ được hoàn trả sau khi kiểm tra xe không có hư hỏng.</li>");
            sb.AppendLine("<li>Trường hợp xe bị hư hỏng do lỗi của Bên B, chi phí sửa chữa sẽ do Bên B chịu.</li>");
            sb.AppendLine("<li>Bên B không được cho thuê lại xe cho bên thứ ba khi chưa có sự đồng ý của Bên A.</li>");
            sb.AppendLine("<li>Mọi tranh chấp phát sinh sẽ được giải quyết thông qua thương lượng hoặc theo pháp luật Việt Nam.</li>");
            sb.AppendLine("</ul>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
        }

        private void AppendSignatures(StringBuilder sb, ContractDataDto data)
        {
            sb.AppendLine("<div class='signatures'>");

            sb.AppendLine("<div class='signature-box'>");
            sb.AppendLine("<p><strong>BÊN CHO THUÊ</strong></p>");
            sb.AppendLine($"<p>{data.CompanyRepresentative}</p>");
            sb.AppendLine("<div class='signature-line'>Chữ ký</div>");
            sb.AppendLine("</div>");

            sb.AppendLine("<div class='signature-box'>");
            sb.AppendLine("<p><strong>BÊN THUÊ</strong></p>");
            sb.AppendLine($"<p>{data.CustomerName}</p>");
            sb.AppendLine("<div class='signature-line'>Chữ ký điện tử</div>");
            sb.AppendLine($"<p style='font-size: 12px; color: #666; margin-top: 10px;'>Đã ký điện tử lúc {data.PaymentDate:HH:mm dd/MM/yyyy}</p>");
            sb.AppendLine("</div>");

            sb.AppendLine("</div>");
        }

        #endregion

        #region Private Helpers - Email

        /// <summary>
        /// Gửi email hợp đồng cho khách hàng
        /// </summary>
        private async Task SendContractEmailAsync(ContractDataDto data, string pdfPath)
        {
            try
            {
                // Đảm bảo file tồn tại
                if (!File.Exists(pdfPath))
                {
                    _logger.LogError("PDF file not found at {PdfPath}", pdfPath);
                    throw new FileNotFoundException($"Không tìm thấy file hợp đồng tại: {pdfPath}");
                }

                // Gọi EmailService để gửi email với attachment
                var emailSent = await _emailService.SendContractEmailAsync(
                    toEmail: data.CustomerEmail,
                    customerName: data.CustomerName,
                    contractNumber: data.ContractNumber,
                    absoluteFilePath: pdfPath);

                if (emailSent)
                {
                    _logger.LogInformation(
                        "Contract email sent successfully to {Email} for Order {OrderId}, Contract {ContractNumber}",
                        data.CustomerEmail, data.OrderId, data.ContractNumber);
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to send contract email to {Email} for Order {OrderId}",
                        data.CustomerEmail, data.OrderId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error sending contract email to {Email} for Order {OrderId}",
                    data.CustomerEmail, data.OrderId);
                // ✅ Không throw - email fail không ảnh hưởng contract creation
            }
        }

        #endregion

        #region Private Helpers - Utilities

        /// <summary>
        /// Tạo số hợp đồng duy nhất
        /// Format: CT-20251021-000123
        /// </summary>
        private string GenerateContractNumber(int orderId)
        {
            var prefix = _contractSettings.NumberPrefix;
            var dateFormat = _contractSettings.DateFormat;
            var padding = _contractSettings.OrderIdPadding;

            return $"{prefix}-{DateTime.UtcNow.ToString(dateFormat)}-{orderId.ToString($"D{padding}")}";
        }

        /// <summary>
        /// Fill default company info từ settings nếu FE chưa gửi
        /// </summary>
        private void FillDefaultCompanyInfo(ContractDataDto data)
        {
            if (string.IsNullOrWhiteSpace(data.CompanyName))
                data.CompanyName = _contractSettings.CompanyName;

            if (string.IsNullOrWhiteSpace(data.CompanyAddress))
                data.CompanyAddress = _contractSettings.CompanyAddress;

            if (string.IsNullOrWhiteSpace(data.CompanyTaxCode))
                data.CompanyTaxCode = _contractSettings.CompanyTaxCode;

            if (string.IsNullOrWhiteSpace(data.CompanyRepresentative))
                data.CompanyRepresentative = _contractSettings.CompanyRepresentative;
        }

        /// <summary>
        /// Tạo URL download hợp đồng
        /// </summary>
        private string GenerateDownloadUrl(string pdfPath)
        {
            var fileName = Path.GetFileName(pdfPath);
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://api.example.com";
            return $"{baseUrl}/api/contracts/download?file={Uri.EscapeDataString(fileName)}";
        }

        #endregion

        #region Contract Download & Retrieval

        /// <summary>
        /// Get contract file for download with security validation
        /// </summary>
        public async Task<(byte[] FileBytes, string FileName, string ContentType)> GetContractFileAsync(
            string fileName, int userId, string userRole)
        {
            _logger.LogInformation(
                "Contract file download requested: {FileName} by User {UserId} (Role: {UserRole})",
                fileName, userId, userRole);

            // 1. Validate file name (prevent path traversal)
            if (string.IsNullOrWhiteSpace(fileName) ||
                fileName.Contains("..") ||
                fileName.Contains("/") ||
                fileName.Contains("\\"))
            {
                throw new ArgumentException("Invalid file name");
            }

            // 2. Only allow PDF files
            if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Only PDF files are allowed");
            }

            // 3. Build file path
            var contractsPath = Path.Combine(Directory.GetCurrentDirectory(), _contractSettings.StoragePath);
            var filePath = Path.Combine(contractsPath, fileName);

            // 4. Check file exists
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Contract file not found: {FilePath}", filePath);
                throw new FileNotFoundException($"Contract file not found: {fileName}");
            }

            // 5. Security check: Member can only download their own contracts
            if (userRole == "Member")
            {
                // Extract contract number from filename (e.g., CT20241022000001.pdf)
                var contractNumber = Path.GetFileNameWithoutExtension(fileName);
                var contract = await _contractRepo.GetByContractNumberAsync(contractNumber);

                if (contract == null)
                {
                    _logger.LogWarning("Contract not found in database: {ContractNumber}", contractNumber);
                    throw new FileNotFoundException("Contract not found");
                }

                // Get order to verify ownership
                var order = contract.Order;
                if (order == null || order.UserId != userId)
                {
                    _logger.LogWarning(
                        "User {UserId} attempted to download contract {ContractNumber} owned by User {OwnerId}",
                        userId, contractNumber, order?.UserId);
                    throw new UnauthorizedAccessException("You do not have permission to download this contract");
                }
            }
            // Employee and Admin can download any contract (no additional check needed)

            // 6. Read file bytes
            var fileBytes = await File.ReadAllBytesAsync(filePath);

            _logger.LogInformation(
                "Contract file downloaded successfully: {FileName} ({FileSize} bytes)",
                fileName, fileBytes.Length);

            return (fileBytes, fileName, "application/pdf");
        }

        /// <summary>
        /// Get contract details by OrderId with ownership validation
        /// </summary>
        public async Task<ContractDetailsDto> GetContractByOrderIdAsync(
            int orderId, int userId, string userRole)
        {
            _logger.LogInformation(
                "Contract requested for Order {OrderId} by User {UserId} (Role: {UserRole})",
                orderId, userId, userRole);

            // 1. Get contract from database
            var contract = await _contractRepo.GetByOrderIdAsync(orderId);

            if (contract == null)
            {
                _logger.LogWarning("Contract not found for Order {OrderId}", orderId);
                throw new InvalidOperationException($"Contract not found for Order {orderId}");
            }

            // 2. Security check: Member can only view their own contracts
            if (userRole == "Member")
            {
                var order = contract.Order;
                if (order == null || order.UserId != userId)
                {
                    _logger.LogWarning(
                        "User {UserId} attempted to view contract for Order {OrderId} owned by User {OwnerId}",
                        userId, orderId, order?.UserId);
                    throw new UnauthorizedAccessException("You do not have permission to view this contract");
                }
            }
            // Employee and Admin can view any contract (no additional check needed)

            // 3. Return contract details
            var contractDto = MapToDetailsDto(contract);

            _logger.LogInformation(
                "Contract details retrieved successfully for Order {OrderId}: {ContractNumber}",
                orderId, contractDto.ContractNumber);

            return contractDto;
        }

        #endregion
    }
}