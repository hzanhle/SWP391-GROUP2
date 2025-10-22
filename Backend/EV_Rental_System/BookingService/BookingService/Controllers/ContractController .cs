using Microsoft.AspNetCore.Mvc;
using BookingService.DTOs;
using BookingService.Services;
using Microsoft.AspNetCore.Authorization;

namespace BookingService.Controllers
{
    /// <summary>
    /// ⭐ CONTROLLER MỚI - Frontend gọi để tạo hợp đồng sau khi thanh toán thành công
    /// </summary>
    [ApiController]
    [Route("api/contracts")]
    [Authorize] // Yêu cầu authentication cho tất cả endpoints
    public class ContractController : ControllerBase
    {
        private readonly IOnlineContractService _contractService;
        private readonly ILogger<ContractController> _logger;

        public ContractController(
            IOnlineContractService contractService,
            ILogger<ContractController> logger)
        {
            _contractService = contractService;
            _logger = logger;
        }

        /// <summary>
        /// ⭐ ENDPOINT CHÍNH - Tạo hợp đồng từ ContractDataDto
        /// 
        /// LUỒNG:
        /// 1. User thanh toán thành công → VNPay webhook → Backend ConfirmPayment
        /// 2. Backend gửi SignalR "PaymentSuccess" { OrderId, TransactionId }
        /// 3. Frontend nhận SignalR → Thu thập data:
        ///    - UserDto (từ user context)
        ///    - VehicleDto (từ vehicle selection)
        ///    - OrderPreviewResponse (từ preview step)
        ///    - TransactionId (từ SignalR event)
        /// 4. Frontend gộp thành ContractDataDto → POST đến endpoint này
        /// 5. Backend validate → Generate PDF → Save DB → Send email → Return response
        /// 
        /// Chỉ Member (khách hàng) mới được tạo hợp đồng
        /// </summary>
        [HttpPost("create")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> CreateContract([FromBody] ContractDataDto contractData)
        {
            try
            {
                _logger.LogInformation(
                    "Received contract creation request for Order {OrderId}, Customer {CustomerName}",
                    contractData.OrderId, contractData.CustomerName);

                // OnlineContractService xử lý toàn bộ:
                // - Validate data
                // - Generate contract number
                // - Fill default company info
                // - Generate PDF from HTML
                // - Save to database
                // - Send email (background task)
                var contractDetails = await _contractService.CreateContractFromDataAsync(contractData);

                _logger.LogInformation(
                    "Contract created successfully for Order {OrderId}, Contract {ContractNumber}",
                    contractData.OrderId, contractDetails.ContractNumber);

                return Ok(contractDetails);
            }
            catch (InvalidOperationException ex)
            {
                // Validation errors hoặc business logic errors
                _logger.LogWarning(ex,
                    "Validation failed for contract creation, Order {OrderId}",
                    contractData.OrderId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (FileNotFoundException ex)
            {
                // PDF generation failed
                _logger.LogError(ex,
                    "File not found during contract creation for Order {OrderId}",
                    contractData.OrderId);
                return StatusCode(500, new { Message = "Lỗi khi tạo file PDF hợp đồng." });
            }
            catch (Exception ex)
            {
                // Unexpected errors
                _logger.LogError(ex,
                    "Unexpected error creating contract for Order {OrderId}",
                    contractData.OrderId);
                return StatusCode(500, new { Message = "Lỗi hệ thống khi tạo hợp đồng." });
            }
        }

        /// <summary>
        /// Download hợp đồng PDF
        /// Member xem/tải hợp đồng của mình
        /// Employee/Admin xem/tải bất kỳ hợp đồng nào
        /// </summary>
        [HttpGet("download")]
        [Authorize(Roles = "Admin,Employee,Member")]
        public async Task<IActionResult> DownloadContract([FromQuery] string file)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(file))
                {
                    return BadRequest(new { Message = "Tên file không hợp lệ." });
                }

                // TODO: Implement file download logic
                // - Validate file exists
                // - Check user permission (Member chỉ tải file của mình)
                // - Return FileStreamResult

                _logger.LogInformation("Contract download requested: {FileName}", file);

                return NotFound(new { Message = "Tính năng download đang được phát triển." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading contract file: {FileName}", file);
                return StatusCode(500, new { Message = "Lỗi khi tải xuống hợp đồng." });
            }
        }

        /// <summary>
        /// Lấy thông tin hợp đồng theo OrderId
        /// Member xem hợp đồng của mình
        /// Employee/Admin xem bất kỳ hợp đồng nào
        /// </summary>
        [HttpGet("order/{orderId}")]
        [Authorize(Roles = "Admin,Employee,Member")]
        public async Task<IActionResult> GetContractByOrderId(int orderId)
        {
            try
            {
                // TODO: Implement get contract logic
                // var contract = await _contractService.GetContractByOrderIdAsync(orderId);
                // Service phải validate: Member chỉ xem được hợp đồng của đơn hàng mình

                _logger.LogInformation("Contract requested for Order {OrderId}", orderId);

                return NotFound(new { Message = "Tính năng đang được phát triển." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contract for Order {OrderId}", orderId);
                return StatusCode(500, new { Message = "Lỗi hệ thống." });
            }
        }
    }
}

/*
 * ===== PHÂN QUYỀN CONTRACT CONTROLLER =====
 * 
 * 🔐 MEMBER (Khách hàng):
 *    - POST /create: Tạo hợp đồng sau khi thanh toán thành công
 *    - GET /download: Tải hợp đồng của mình
 *    - GET /order/{orderId}: Xem hợp đồng của đơn hàng mình
 * 
 * 👔 EMPLOYEE (Nhân viên):
 *    - GET /download: Tải bất kỳ hợp đồng nào
 *    - GET /order/{orderId}: Xem bất kỳ hợp đồng nào
 * 
 * 👑 ADMIN (Quản trị viên):
 *    - Tất cả quyền của Employee
 * 
 * ⚠️ LƯU Ý QUAN TRỌNG:
 * 1. Service layer PHẢI validate ownership:
 *    - Member chỉ được tạo/xem hợp đồng của đơn hàng mình
 *    - Kiểm tra userId từ JWT token vs userId trong Order
 * 
 * 2. File security:
 *    - Download endpoint phải validate file path (prevent path traversal)
 *    - Chỉ cho phép download file PDF trong thư mục contracts
 *    - Member chỉ download được file của mình
 * 
 * ===== CÁCH SỬ DỤNG (FRONTEND) =====
 * 
 * // 1. Connect SignalR
 * hubConnection.on("PaymentSuccess", async (data) => {
 *   const { OrderId, TransactionId } = data;
 *   
 *   // 2. Thu thập data từ các nguồn
 *   const contractData = {
 *     // Order info
 *     orderId: OrderId,
 *     
 *     // Customer info (từ user context)
 *     customerName: currentUser.fullName,
 *     customerEmail: currentUser.email,
 *     customerPhone: currentUser.phone,
 *     customerIdCard: currentUser.idCard,
 *     customerAddress: currentUser.address,
 *     customerDateOfBirth: currentUser.dateOfBirth,
 *     
 *     // Vehicle info (từ vehicle selection)
 *     vehicleModel: selectedVehicle.model,
 *     licensePlate: selectedVehicle.licensePlate,
 *     vehicleColor: selectedVehicle.color,
 *     vehicleType: selectedVehicle.type,
 *     
 *     // Rental info (từ order preview)
 *     fromDate: orderPreview.fromDate,
 *     toDate: orderPreview.toDate,
 *     
 *     // Financial info (từ order preview)
 *     totalRentalCost: orderPreview.totalRentalCost,
 *     depositAmount: orderPreview.depositAmount,
 *     serviceFee: orderPreview.serviceFee,
 *     totalPaymentAmount: orderPreview.totalPaymentAmount,
 *     
 *     // Payment info
 *     transactionId: TransactionId,  // ← Từ SignalR event!
 *     paymentMethod: "VNPay",
 *     paymentDate: new Date()
 *   };
 *   
 *   // 3. Gọi API tạo contract (kèm JWT token)
 *   const response = await fetch('/api/contracts/create', {
 *     method: 'POST',
 *     headers: { 
 *       'Content-Type': 'application/json',
 *       'Authorization': `Bearer ${jwtToken}` // ← QUAN TRỌNG!
 *     },
 *     body: JSON.stringify(contractData)
 *   });
 *   
 *   const contract = await response.json();
 *   
 *   // 4. Hiển thị thành công + link download
 *   showSuccessMessage(contract.message);
 *   showDownloadLink(contract.downloadUrl);
 * });
 */