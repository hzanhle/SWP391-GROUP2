using BookingService.DTOs;
using BookingService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookingService.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize] // Yêu cầu authentication cho tất cả endpoints
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IOrderService orderService,
            ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
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
                return StatusCode(500, new { Message = "Lỗi h�� thống khi tạo đơn hàng." });
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
        /// Lấy thông tin chi tiết đơn hàng (including OnlineContract)
        /// Admin, Employee có thể xem tất cả đơn
        /// Member chỉ xem được đơn của mình (validate trong service)
        /// </summary>
        [HttpGet("{orderId}")]
        [Authorize(Roles = "Admin,Employee,Member")]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            try
            {
                var order = await _orderService.GetOrderByIdWithDetailsAsync(orderId);
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
        /// Bắt đầu chuyến thuê (khi khách nhận xe)
        /// Chủ xe (Member) hoặc Employee có thể xác nhận bắt đầu
        /// </summary>
        [HttpPost("{orderId}/start")]
        [Authorize(Roles = "Employee,Member")]
        public async Task<IActionResult> StartRental(int orderId)
        {
            try
            {
                var success = await _orderService.StartRentalAsync(orderId);
                if (!success)
                {
                    return BadRequest(new { Message = "Không thể bắt đầu chuyến thuê." });
                }
                return Ok(new { Message = "Chuyến thuê đã bắt đầu." });
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
        /// Hoàn thành chuyến thuê (khi khách trả xe)
        /// Chủ xe (Member) hoặc Employee có thể xác nhận hoàn thành
        /// </summary>
        [HttpPost("{orderId}/complete")]
        [Authorize(Roles = "Employee,Member")]
        public async Task<IActionResult> CompleteRental(int orderId)
        {
            try
            {
                var success = await _orderService.CompleteRentalAsync(orderId);
                if (!success)
                {
                    return BadRequest(new { Message = "Không thể hoàn thành chuyến thuê." });
                }
                return Ok(new { Message = "Chuyến thuê đã hoàn thành." });
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
    }
}
