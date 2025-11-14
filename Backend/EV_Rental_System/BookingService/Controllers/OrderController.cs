using BookingService.DTOs;
using BookingService.Services;
using BookingService.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BookingService.Models;

namespace BookingService.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize] // Yêu cầu authentication cho tất cả endpoints
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ISettlementRepository _settlementRepo;
        private readonly ITrustScoreHistoryRepository _trustScoreHistoryRepo;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OrderController> _logger;
        private readonly IConfiguration _configuration;

        public OrderController(
            IOrderService orderService,
            ISettlementRepository settlementRepo,
            ITrustScoreHistoryRepository trustScoreHistoryRepo,
            IHttpClientFactory httpClientFactory,
            ILogger<OrderController> logger,
            IConfiguration configuration)
        {
            _orderService = orderService;
            _settlementRepo = settlementRepo;
            _trustScoreHistoryRepo = trustScoreHistoryRepo;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
        }
        /// <summary>
        /// Lấy UserId từ JWT token trong header Authorization
        /// </summary>
        private int GetUserIdFromToken()
        {
            // Lấy claim userId từ JWT đã được validate bởi [Authorize]
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("userId")
                ?? User.FindFirst("sub");

            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                throw new UnauthorizedAccessException("Không thể trích xuất UserId từ token.");
            }

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("UserId trong token không hợp lệ.");
            }

            return userId;
        }
        /// <summary>
        /// Xem trước đơn hàng: tính toán chi phí và kiểm tra lịch.
        /// Chỉ Member mới có thể xem trước đơn hàng của mình
        /// </summary>
        [HttpPost("preview")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> GetOrderPreview([FromBody] OrderRequest request)
        {
            int userId = GetUserIdFromToken();
            try
            {
                var preview = await _orderService.GetOrderPreviewAsync(request, userId);
                return Ok(preview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order preview for User {UserId}", userId);
                return BadRequest(new { Message = "Lỗi khi xem trước đơn hàng: " + ex.Message });
            }
        }

        /// <summary>
        /// Tạo đơn hàng mới (trạng thái Pending).
        /// Chỉ Member mới có thể tạo đơn hàng
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequest request)
        {
            var userId = GetUserIdFromToken();
            try
            {
                var order = await _orderService.CreateOrderAsync(request, userId);
                return Ok(order);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when creating order for User {UserId}", userId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for User {UserId}", userId);
                return StatusCode(500, new { Message = "Lỗi hệ thống khi tạo đơn hàng." });
            }
        }

        /// <summary>
        /// ⭐ ĐÃ FIX - Xác nhận thanh toán từ webhook cổng thanh toán (VNPay).
        /// QUAN TRỌNG: Endpoint này chỉ dành cho webhook từ payment gateway!
        /// AllowAnonymous vì webhook từ bên thứ 3 không có JWT token
        /// Nên validate bằng signature/secret key trong service layer
        /// </summary>
        [HttpPost("confirm-payment")]
        [AllowAnonymous] // Webhook từ VNPay không có token
        public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "Received payment confirmation for Order {OrderId}, TransactionId: {TransactionId}",
                    request.OrderId, request.TransactionId);

                // ✅ OrderService.ConfirmPaymentAsync đã xử lý TẤT CẢ:
                // - Update Order status → Confirmed
                // - Update Payment status → Completed
                // - Update Trust Score
                // - Insert Notification record
                // - Send SignalR event (với TransactionId)
                //
                // ✅ ĐÃ XÓA:
                // - Auto contract generation (Frontend sẽ gọi riêng)
                // - Duplicate calls đến các services khác
                var success = await _orderService.ConfirmPaymentAsync(
                    request.OrderId,
                    request.TransactionId,
                    request.GatewayResponse);

                if (!success)
                {
                    return BadRequest(new { Message = "Xác nhận thanh toán thất bại." });
                }

                return Ok(new
                {
                    Message = "Thanh toán xác nhận thành công.",
                    OrderId = request.OrderId,
                    TransactionId = request.TransactionId
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid payment confirmation for Order {OrderId}", request.OrderId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming payment for Order {OrderId}", request.OrderId);
                return StatusCode(500, new { Message = "Lỗi hệ thống khi xác nhận thanh toán." });
            }
        }

        /// <summary>
        /// Background job kiểm tra đơn hàng hết hạn.
        /// Thường được gọi bởi scheduler (Hangfire/Quartz), không phải từ client.
        /// Chỉ Admin hoặc hệ thống mới có thể gọi
        /// </summary>
        [HttpPost("check-expired")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CheckExpiredOrders()
        {
            try
            {
                // OrderService.CheckExpiredOrdersAsync đã xử lý TẤT CẢ:
                // - Cancel expired orders
                // - Insert notification records
                // - Send SignalR events
                var expiredCount = await _orderService.CheckExpiredOrdersAsync();

                _logger.LogInformation("Expired orders check completed. Processed: {Count}", expiredCount);

                return Ok(new
                {
                    Message = $"{expiredCount} đơn hàng đã được xử lý hết hạn.",
                    ProcessedCount = expiredCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking expired orders");
                return StatusCode(500, new { Message = "Lỗi hệ thống khi kiểm tra đơn hàng hết hạn." });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết đơn hàng
        /// Admin, Employee có thể xem tất cả đơn
        /// Member chỉ xem được đơn của mình (validate trong service)
        /// </summary>
        [HttpGet("{orderId}")]
        [Authorize(Roles = "Admin,Employee,Member")]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    return NotFound(new { Message = $"Không tìm thấy đơn hàng {orderId}." });
                }
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order {OrderId}", orderId);
                return StatusCode(500, new { Message = "Lỗi hệ thống." });
            }
        }

        /// <summary>
        /// Lấy danh sách đơn hàng của user
        /// Admin, Employee có thể xem đơn của bất kỳ user nào
        /// Member chỉ xem được đơn của mình (validate trong service)
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin,Employee,Member")]
        public async Task<IActionResult> GetOrdersByUserId(int userId)
        {
            try
            {
                var orders = await _orderService.GetOrdersByUserIdAsync(userId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders for User {UserId}", userId);
                return StatusCode(500, new { Message = "Lỗi hệ thống." });
            }
        }

        /// <summary>
        /// Bắt đầu chuyến thuê (khi khách nhận xe) - Có upload ảnh xe
        /// Chủ xe (Member) hoặc Employee có thể xác nhận bắt đầu
        /// </summary>
        [HttpPost("{orderId}/start")]
        [Authorize(Roles = "Employee,Member")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> StartRental(
            int orderId,
            [FromForm] VehicleCheckInRequest request,
            [FromForm] List<IFormFile> images)
        {
            try
            {
                // Get current user ID from JWT token
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int confirmedBy))
                {
                    return Unauthorized(new { Message = "Không thể xác thực người dùng." });
                }

                // Validate images
                if (images == null || images.Count == 0)
                {
                    return BadRequest(new { Message = "Phải có ít nhất một ảnh xe để bắt đầu thuê." });
                }

                var success = await _orderService.StartRentalAsync(orderId, images, confirmedBy, request);
                if (!success)
                {
                    return BadRequest(new { Message = "Không thể bắt đầu chuyến thuê." });
                }

                return Ok(new { Message = "Chuyến thuê đã bắt đầu. Ảnh xe đã được lưu." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when starting rental for Order {OrderId}", orderId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting rental for Order {OrderId}", orderId);
                return StatusCode(500, new { Message = "Lỗi hệ thống." });
            }
        }

        /// <summary>
        /// Hoàn thành chuyến thuê (khi khách trả xe) - Có upload ảnh xe
        /// Chủ xe (Member) hoặc Employee có thể xác nhận hoàn thành
        /// </summary>
        [HttpPost("{orderId}/complete")]
        [Authorize(Roles = "Employee,Member")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CompleteRental(
            int orderId,
            [FromForm] VehicleReturnRequest request,
            [FromForm] List<IFormFile> images)
        {
            try
            {
                // Get current user ID from JWT token
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int confirmedBy))
                {
                    return Unauthorized(new { Message = "Không thể xác thực người dùng." });
                }

                // Validate images
                if (images == null || images.Count == 0)
                {
                    return BadRequest(new { Message = "Phải có ít nhất một ảnh xe để hoàn thành thuê." });
                }

                var success = await _orderService.CompleteRentalAsync(orderId, images, confirmedBy, request);
                if (!success)
                {
                    return BadRequest(new { Message = "Không thể hoàn thành chuyến thuê." });
                }

                var message = request.HasDamage
                    ? "Chuyến thuê đã hoàn thành. Ảnh xe và thông tin hư hỏng đã được lưu."
                    : "Chuyến thuê đã hoàn thành. Ảnh xe đã được lưu.";

                return Ok(new { Message = message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when completing rental for Order {OrderId}", orderId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing rental for Order {OrderId}", orderId);
                return StatusCode(500, new { Message = "Lỗi hệ thống." });
            }
        }

        /// <summary>
        /// User views their own rental history
        /// Returns all completed rentals for the authenticated user
        /// </summary>
        [HttpGet("my-history")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> GetMyRentalHistory()
        {
            try
            {
                // Get userId from JWT token
                var userIdClaim = User.FindFirst("userId") ?? User.FindFirst("sub");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { Message = "Invalid user token" });
                }

                var orders = await _orderService.GetOrdersByUserIdAsync(userId);

                // Filter to only show completed rentals
                var completedOrders = orders
                    .Where(o => o.Status == Models.OrderStatus.Completed)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToList();

                // Map to RentalHistoryItemResponse DTOs
                var history = new List<RentalHistoryItemResponse>();

                foreach (var order in completedOrders)
                {
                    // Get settlement data
                    var settlement = await _settlementRepo.GetByOrderIdAsync(order.OrderId);

                    // Get trust score impact for this order
                    var trustScoreHistory = await _trustScoreHistoryRepo.GetByOrderIdAsync(order.OrderId);
                    var trustScoreImpact = trustScoreHistory.Sum(h => h.ChangeAmount);

                    // Fetch vehicle name from TwoWheelVehicleService
                    var vehicleName = await GetVehicleNameAsync(order.VehicleId);

                    history.Add(new RentalHistoryItemResponse
                    {
                        OrderId = order.OrderId,
                        VehicleId = order.VehicleId,
                        VehicleName = vehicleName ?? $"Vehicle #{order.VehicleId}",
                        FromDate = order.FromDate,
                        ToDate = order.ToDate,
                        ActualReturnTime = settlement?.ActualReturnTime,
                        TotalCost = order.TotalCost,
                        DepositAmount = order.DepositAmount,
                        Status = order.Status.ToString(),
                        IsLate = settlement?.OvertimeHours > 0,
                        HasDamage = settlement?.DamageCharge > 0,
                        TrustScoreImpact = trustScoreImpact,
                        OvertimeFee = settlement?.OvertimeFee,
                        DamageCharge = settlement?.DamageCharge,
                        DepositRefundAmount = settlement?.DepositRefundAmount,
                        AdditionalPaymentRequired = settlement?.AdditionalPaymentRequired,
                        CreatedAt = order.CreatedAt
                    });
                }

                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rental history for current user");
                return StatusCode(500, new { Message = "Lỗi khi lấy lịch sử thuê xe." });
            }
        }

        /// <summary>
        /// Admin/Employee views any user's rental history
        /// </summary>
        [HttpGet("history/user/{userId}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetUserRentalHistory(int userId)
        {
            try
            {
                var orders = await _orderService.GetOrdersByUserIdAsync(userId);

                // Show all completed orders for admin
                var completedOrders = orders
                    .Where(o => o.Status == Models.OrderStatus.Completed)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToList();

                // Map to RentalHistoryItemResponse DTOs
                var history = new List<RentalHistoryItemResponse>();

                foreach (var order in completedOrders)
                {
                    // Get settlement data
                    var settlement = await _settlementRepo.GetByOrderIdAsync(order.OrderId);

                    // Get trust score impact for this order
                    var trustScoreHistory = await _trustScoreHistoryRepo.GetByOrderIdAsync(order.OrderId);
                    var trustScoreImpact = trustScoreHistory.Sum(h => h.ChangeAmount);

                    // Fetch vehicle name from TwoWheelVehicleService
                    var vehicleName = await GetVehicleNameAsync(order.VehicleId);

                    history.Add(new RentalHistoryItemResponse
                    {
                        OrderId = order.OrderId,
                        VehicleId = order.VehicleId,
                        VehicleName = vehicleName ?? $"Vehicle #{order.VehicleId}",
                        FromDate = order.FromDate,
                        ToDate = order.ToDate,
                        ActualReturnTime = settlement?.ActualReturnTime,
                        TotalCost = order.TotalCost,
                        DepositAmount = order.DepositAmount,
                        Status = order.Status.ToString(),
                        IsLate = settlement?.OvertimeHours > 0,
                        HasDamage = settlement?.DamageCharge > 0,
                        TrustScoreImpact = trustScoreImpact,
                        OvertimeFee = settlement?.OvertimeFee,
                        DamageCharge = settlement?.DamageCharge,
                        DepositRefundAmount = settlement?.DepositRefundAmount,
                        AdditionalPaymentRequired = settlement?.AdditionalPaymentRequired,
                        CreatedAt = order.CreatedAt
                    });
                }

                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rental history for User {UserId}", userId);
                return StatusCode(500, new { Message = "Lỗi khi lấy lịch sử thuê xe." });
            }
        }

        /// <summary>
        /// Helper method to fetch vehicle name from TwoWheelVehicleService
        /// </summary>
        private async Task<string?> GetVehicleNameAsync(int vehicleId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var twoWheelServiceUrl = _configuration["ServiceUrls:TwoWheelVehicleService"]
                    ?? "http://localhost:5051";

                var response = await client.GetAsync($"{twoWheelServiceUrl}/api/vehicles/{vehicleId}");

                if (response.IsSuccessStatusCode)
                {
                    var vehicleData = await response.Content.ReadFromJsonAsync<VehicleResponse>();
                    return vehicleData?.ModelName;
                }

                _logger.LogWarning("Failed to fetch vehicle name for VehicleId {VehicleId}", vehicleId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching vehicle name for VehicleId {VehicleId}", vehicleId);
                return null;
            }
        }

        /// <summary>
        /// Lấy báo cáo giờ cao điểm đặt xe
        /// </summary>
        /// <returns>Thống kê số lượng đơn theo từng giờ trong ngày và top giờ cao điểm</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("reports/peak-hours")]
        [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult<ResponseDTO>> GetPeakHoursReport()
        {
            try
            {
                var report = await _orderService.GetPeakHoursReportAsync();

                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Lấy báo cáo giờ cao điểm thành công",
                    Data = report
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Lỗi khi tạo báo cáo giờ cao điểm",
                    Data = ex.Message
                });
            }
        }

        [HttpPut]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateOrder([FromBody] Order request)
        {
            try
            {
                await _orderService.UpdateOrderAsync(request);
                return Ok(new { Message = "Cập nhật đơn hàng thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId}", request.OrderId);
                return StatusCode(500, new { Message = "Lỗi hệ thống khi cập nhật đơn hàng." });
            }
        }

        /// <summary>
        /// Temporary DTO for vehicle response
        /// </summary>
        private class VehicleResponse
        {
            public string? ModelName { get; set; }
        }
    }
}
