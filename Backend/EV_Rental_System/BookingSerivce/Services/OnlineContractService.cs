using BookingService.Models;
using BookingService.DTOs; // Cần ContractBindingData, ContractDetailsDto, OverallContractStatus
using BookingService.Repositories;
using System.Globalization;
using System.Text.Json;
// using iText.Forms; // (Thư viện PDF của bạn)
// using iText.Kernel.Pdf;

namespace BookingService.Services
{
    public class OnlineContractService : IOnlineContractService
    {
        private readonly IOnlineContractRepository _contractRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly IPaymentRepository _paymentRepo; // <-- (MỚI) Cần để lấy trạng thái Payment
        private readonly IEmailService _emailService;
        private readonly ILogger<OnlineContractService> _logger;
        private readonly string _templatePath;
        private readonly string _outputPath;

        public OnlineContractService(
            IOnlineContractRepository contractRepo,
            IOrderRepository orderRepo,
            IPaymentRepository paymentRepo, // <-- (MỚI) Inject Repo Payment
            IEmailService emailService,
            ILogger<OnlineContractService> logger,
            IConfiguration config)
        {
            _contractRepo = contractRepo;
            _orderRepo = orderRepo;
            _paymentRepo = paymentRepo; // <-- (MỚI) Gán Repo Payment
            _emailService = emailService;
            _logger = logger;
            _templatePath = config.GetValue<string>("ContractPaths:TemplatePath") ?? "Templates/template.pdf";
            _outputPath = config.GetValue<string>("ContractPaths:OutputPath") ?? "Storage/Contracts";
        }

        // --- GENERATE CONTRACT (Logic đã OK) ---
        public async Task<OnlineContract> GenerateContractOnPaymentSuccessAsync(
            Payment completedPayment,
            string contractDataJson)
        {
            int orderId = completedPayment.OrderId;
            _logger.LogInformation($"Bắt đầu tạo hợp đồng cho Order {orderId}...");

            // 1. Kiểm tra Idempotency
            if (await _contractRepo.ExistsByOrderIdAsync(orderId))
            {
                _logger.LogWarning($"Hợp đồng cho Order {orderId} đã tồn tại. Bỏ qua.");
                // Trả về hợp đồng đã tồn tại thay vì lỗi
                var existingContract = await _contractRepo.GetByOrderIdAsync(orderId);
                // Có thể log thêm thông tin về payment hiện tại và payment cũ nếu cần debug
                return existingContract ?? throw new InvalidOperationException($"Contract exists but couldn't be retrieved for Order {orderId}");
            }

            // 2. Tổ hợp 3 biến tham chiếu
            var feData = JsonSerializer.Deserialize<ContractBindingData>(contractDataJson);
            if (feData == null) { /* Log Error & Throw JsonException */ }

            var beOrderData = await _orderRepo.GetByIdAsync(orderId);
            if (beOrderData == null) { /* Log Error & Throw KeyNotFoundException */ }

            var bePaymentData = completedPayment; // Đã được truyền vào

            // 3. Binding PDF
            var contractNumber = $"CT-{DateTime.UtcNow:yyyyMMdd}-{orderId:D5}";
            var outputFileName = $"{contractNumber}.pdf";
            var absoluteStorageDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", _outputPath);
            Directory.CreateDirectory(absoluteStorageDir);
            var absoluteFilePath = Path.Combine(absoluteStorageDir, outputFileName);

            await FillPdfTemplateAsync(absoluteFilePath, contractNumber, feData, beOrderData, bePaymentData);

            // 4. Lưu OnlineContract vào DB
            var relativeFilePath = Path.Combine(_outputPath, outputFileName).Replace("\\", "/");
            var newContract = new OnlineContract(
                orderId: orderId,
                contractNumber: contractNumber,
                filePath: relativeFilePath,
                signedAt: bePaymentData.PaidAt ?? DateTime.UtcNow, // Use PaidAt, fallback to UtcNow if somehow null
                signatureData: bePaymentData.TransactionId ?? "N/A" // Use TransactionId
            );
            var savedContract = await _contractRepo.CreateAsync(newContract);
            // Giả sử CreateAsync trả về entity đã lưu (có ID)

            // 5. Gửi Email
            try
            {
                await _emailService.SendContractEmailAsync(
                    feData.CustomerEmail, feData.CustomerName,
                    savedContract.ContractNumber, absoluteFilePath);
                _logger.LogInformation($"Đã yêu cầu gửi email HĐ {contractNumber} đến {feData.CustomerEmail}.");
            }
            catch (Exception ex) { /* Log Error, không throw */ }

            _logger.LogInformation($"Đã tạo hợp đồng {contractNumber} thành công (ID: {savedContract.OnlineContractId}).");
            return savedContract;
        }

        // --- FILL PDF HELPER (Giữ nguyên logic binding) ---
        private async Task FillPdfTemplateAsync(
            string outputPath, string contractNumber,
            ContractBindingData feData, Order beOrderData, Payment bePaymentData)
        {
            await Task.Run(() =>
            {
                _logger.LogDebug($"Bắt đầu binding PDF: {outputPath}");
                var culture = new CultureInfo("vi-VN");
                try
                {
                    // --- Logic iText 7 (hoặc thư viện PDF bạn chọn) ---
                    /*
                    using (var pdfReader = new PdfReader(_templatePath))
                    using (var pdfWriter = new PdfWriter(outputPath))
                    using (var pdfDoc = new PdfDocument(pdfReader, pdfWriter))
                    {
                       var form = PdfAcroForm.GetAcroForm(pdfDoc, true);
                       var fields = form.GetFormFields();

                       // **DÙNG DATA TỪ FE (feData)**
                       fields.GetValue("CustomerName_Field")?.SetValue(feData.CustomerName ?? "");
                       fields.GetValue("CustomerPhone_Field")?.SetValue(feData.CustomerPhone ?? "");
                       fields.GetValue("CustomerEmail_Field")?.SetValue(feData.CustomerEmail ?? "");
                       fields.GetValue("CitizenId_Field")?.SetValue(feData.CitizenId ?? "");
                       fields.GetValue("VehicleName_Field")?.SetValue(feData.ModelName ?? "");
                       fields.GetValue("LicensePlate_Field")?.SetValue(feData.LicensePlate ?? "");
                       fields.GetValue("VehicleColor_Field")?.SetValue(feData.VehicleColor ?? "");

                       // **DÙNG DATA TỪ BE (beOrderData, bePaymentData)**
                       fields.GetValue("ContractNumber_Field")?.SetValue(contractNumber);
                       fields.GetValue("FromDate_Field")?.SetValue(beOrderData.FromDate.ToString("HH:mm dd/MM/yyyy"));
                       fields.GetValue("ToDate_Field")?.SetValue(beOrderData.ToDate.ToString("HH:mm dd/MM/yyyy"));
                       fields.GetValue("RentalCost_Field")?.SetValue(beOrderData.TotalCost.ToString("N0", culture));
                       fields.GetValue("Deposit_Field")?.SetValue(beOrderData.DepositAmount.ToString("N0", culture));
                       fields.GetValue("TotalAmount_Field")?.SetValue(bePaymentData.Amount.ToString("N0", culture)); // Tổng tiền cuối cùng
                       fields.GetValue("TransactionId_Field")?.SetValue(bePaymentData.TransactionId ?? "N/A");
                       fields.GetValue("SignedDate_Field")?.SetValue(bePaymentData.PaidAt?.ToString("dd/MM/yyyy") ?? DateTime.UtcNow.ToString("dd/MM/yyyy"));

                       // **NÚT CHECKED (từ BE)**
                       fields.GetValue("Signature_Check")?.SetValue("Yes"); // Check vào ô đã ký

                       form.FlattenFields(); // Khóa hợp đồng
                    }
                    */

                    // (Giả lập việc binding)
                    File.WriteAllText(outputPath,
                        $"GIẢ LẬP PDF: HĐ {contractNumber} cho {feData.CustomerName} ({feData.CitizenId}).\n" +
                        $"Xe: {feData.ModelName} ({feData.LicensePlate}).\n" +
                        $"Tổng tiền: {bePaymentData.Amount.ToString("N0", culture)} VND (TxID: {bePaymentData.TransactionId}).");

                    _logger.LogInformation($"Đã binding PDF thành công: {outputPath}");

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi nghiêm trọng khi binding PDF cho HĐ {ContractNumber}", contractNumber);
                    // Cân nhắc: Xóa file outputPath nếu tạo lỗi dở dang?
                    // throw; // Ném lại lỗi để transaction rollback (nếu có)
                }
            });
        }

        // --- GET CONTRACT DETAILS (Implement) ---
        public async Task<ContractDetailsDto?> GetContractDetailsByOrderIdAsync(int orderId)
        {
            _logger.LogDebug("Lấy chi tiết hợp đồng cho Order {OrderId}", orderId);
            var contract = await _contractRepo.GetByOrderIdAsync(orderId);
            if (contract == null)
            {
                _logger.LogWarning("Không tìm thấy hợp đồng cho Order {OrderId}", orderId);
                return null;
            }

            // Lấy thêm Payment để xác định trạng thái cuối cùng (Signed/Refunded)
            var payment = await _paymentRepo.GetByOrderIdAsync(orderId);
            OverallContractStatus status = OverallContractStatus.Signed; // Mặc định
            if (payment?.Status == PaymentStatus.Refunded)
            {
                status = OverallContractStatus.Refunded;
            }
            // (Bạn có thể thêm logic check Failed/Cancelled nếu cần, nhưng thường contract chỉ tồn tại khi Signed)

            var dto = new ContractDetailsDto
            {
                ContractId = contract.OnlineContractId,
                OrderId = contract.OrderId,
                ContractNumber = contract.ContractNumber,
                DownloadUrl = contract.ContractFilePath, // Trả về đường dẫn tương đối
                Status = status,
                CreatedAt = contract.CreatedAt, // Ngày tạo bản ghi contract
                SignedAt = contract.SignedAt,   // Ngày ký (lấy từ bản ghi contract, đã được set khi tạo)
                SignatureData = contract.SignatureData // Txn ID (lấy từ bản ghi contract)
            };

            return dto;
        }

        // --- GET DOWNLOAD URL (Logic đã OK) ---
        public async Task<string?> GetContractDownloadUrlAsync(int orderId, int userId)
        {
            _logger.LogDebug("User {UserId} yêu cầu tải HĐ cho Order {OrderId}", userId, orderId);
            var contract = await _contractRepo.GetByOrderIdAsync(orderId);
            if (contract == null)
            {
                _logger.LogWarning("Yêu cầu tải HĐ thất bại: Không tìm thấy HĐ cho Order {OrderId}", orderId);
                return null;
            }

            // Kiểm tra quyền sở hữu Order
            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order == null || order.UserId != userId)
            {
                _logger.LogWarning("Yêu cầu tải HĐ thất bại: User {UserId} không có quyền truy cập Order {OrderId}", userId, orderId);
                return null;
            }

            // Trả về đường dẫn tương đối để FE xử lý
            _logger.LogInformation("Cung cấp đường dẫn tải HĐ {FilePath} cho User {UserId}", contract.ContractFilePath, userId);
            return contract.ContractFilePath;
        }
    }
}