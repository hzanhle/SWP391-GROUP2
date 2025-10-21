using Microsoft.AspNetCore.Mvc;
using BookingService.DTOs;
using BookingService.Services;

namespace BookingService.Controllers
{
    [ApiController]
    [Route("api/orders")]
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
        /// Xem trước đơn hàng: tính toán chi phí và kiểm tra lịch.
        /// </summary>
        [HttpPost("preview")]
        public async Task<IActionResult> GetOrderPreview([FromBody] OrderRequest request)
        {
            try
            {
                var preview = await _orderService.GetOrderPreviewAsync(request);
                return Ok(preview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order preview for User {UserId}", request.UserId);
                return BadRequest(new { Message = "Lỗi khi xem trước đơn hàng: " + ex.Message });
            }
        }

        /// <summary>
        /// Tạo đơn hàng mới (trạng thái Pending).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequest request)
        {
            try
            {
                var order = await _orderService.CreateOrderAsync(request);
                return Ok(order);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when creating order for User {UserId}", request.UserId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for User {UserId}", request.UserId);
                return StatusCode(500, new { Message = "Lỗi hệ thống khi tạo đơn hàng." });
            }
        }

        /// <summary>
        /// ⭐ ĐÃ FIX - Xác nhận thanh toán từ webhook cổng thanh toán (VNPay).
        /// QUAN TRỌNG: Endpoint này chỉ dành cho webhook từ payment gateway!
        /// </summary>
        [HttpPost("confirm-payment")]
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
        /// </summary>
        [HttpPost("check-expired")]
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
        /// </summary>
        [HttpGet("{orderId}")]
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
        /// </summary>
        [HttpGet("user/{userId}")]
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
        /// </summary>
        [HttpPost("{orderId}/start")]
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
        /// </summary>
        [HttpPost("{orderId}/complete")]
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

/*
 * ===== SUMMARY CÁC THAY ĐỔI =====
 * 
 * 1. ✅ XÓA tất cả duplicate service calls trong ConfirmPayment()
 *    - Không còn gọi _paymentService, _trustScoreService, _notificationService
 *    - OrderService đã xử lý tất cả rồi!
 * 
 * 2. ✅ THÊM proper error handling
 *    - InvalidOperationException → 400 Bad Request
 *    - Generic Exception → 500 Internal Server Error
 *    - Logging đầy đủ
 * 
 * 3. ✅ THÊM các endpoints còn thiếu
 *    - GET /api/orders/{orderId}
 *    - GET /api/orders/user/{userId}
 *    - POST /api/orders/{orderId}/start
 *    - POST /api/orders/{orderId}/complete
 * 
 * 4. ✅ Controller giờ chỉ lo routing và validation
 *    - Business logic hoàn toàn ở OrderService
 *    - Controller mỏng, dễ test
 */