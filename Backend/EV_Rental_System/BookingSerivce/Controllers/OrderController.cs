using BookingSerivce.DTOs;
using BookingSerivce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingSerivce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Create a new booking order
        /// </summary>
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var order = await _orderService.CreateOrderAsync(request);
                return Ok(new
                {
                    success = true,
                    message = "Order created successfully",
                    data = order
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get order by ID
        /// </summary>
        [HttpGet("{orderId}")]
        [Authorize]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null)
                    return NotFound(new
                    {
                        success = false,
                        message = "Order not found"
                    });

                return Ok(new
                {
                    success = true,
                    data = order
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get all orders for a user
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetUserOrders(int userId)
        {
            try
            {
                var orders = await _orderService.GetUserOrdersAsync(userId);
                return Ok(new
                {
                    success = true,
                    data = orders
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Update order status
        /// </summary>
        [HttpPatch("{orderId}/status")]
        [Authorize]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusRequest request)
        {
            try
            {
                var order = await _orderService.UpdateOrderStatusAsync(orderId, request.Status);
                return Ok(new
                {
                    success = true,
                    message = "Order status updated successfully",
                    data = order
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Check vehicle availability for specific dates
        /// </summary>
        [HttpPost("check-availability")]
        public async Task<IActionResult> CheckAvailability([FromBody] CheckAvailabilityRequest request)
        {
            try
            {
                var isAvailable = await _orderService.CheckVehicleAvailabilityAsync(
                    request.VehicleId,
                    request.FromDate,
                    request.ToDate);

                var overlappingOrders = await _orderService.GetOverlappingOrdersAsync(
                    request.VehicleId,
                    request.FromDate,
                    request.ToDate);

                return Ok(new
                {
                    success = true,
                    isAvailable = isAvailable,
                    overlappingOrdersCount = overlappingOrders.Count()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // ========== Stage 1 Enhancement Endpoints ==========

        /// <summary>
        /// Preview an order with calculated costs and trust-score adjusted deposit.
        /// Creates a 5-minute soft lock on the vehicle.
        /// </summary>
        [HttpPost("preview")]
        public async Task<IActionResult> PreviewOrder([FromBody] OrderPreviewRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var preview = await _orderService.PreviewOrderAsync(request);

                return Ok(new
                {
                    success = true,
                    message = "Order preview created successfully. Valid for 5 minutes.",
                    data = preview
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Confirm an order using a preview token.
        /// Validates soft lock and creates the actual order.
        /// </summary>
        [HttpPost("confirm")]
        [Authorize]
        public async Task<IActionResult> ConfirmOrder([FromBody] ConfirmOrderRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var orderResponse = await _orderService.ConfirmOrderAsync(request);

                return Ok(new
                {
                    success = true,
                    message = "Order confirmed successfully. Please proceed to payment within 5 minutes.",
                    data = orderResponse
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }

    public class UpdateOrderStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }

    public class CheckAvailabilityRequest
    {
        public int VehicleId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
