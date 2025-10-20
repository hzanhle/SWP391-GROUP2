using BookingSerivce.DTOs;
using BookingSerivce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        /// Get order by ID with role-based status display (Stage 3 Enhancement)
        /// </summary>
        [HttpGet("{orderId}")]
        [Authorize]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            try
            {
                // Extract user role and userId from JWT claims
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Customer";
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(userIdClaim, out int requestingUserId))
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Invalid user token"
                    });
                }

                // Get role-based order status response
                var orderStatus = await _orderService.GetOrderStatusAsync(orderId, userRole, requestingUserId);

                return Ok(new
                {
                    success = true,
                    data = orderStatus
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
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
        /// No authentication required - users can confirm orders before logging in.
        /// Authentication required at payment stage.
        /// </summary>
        [HttpPost("confirm")]
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

        // ========== Stage 3 Enhancement Endpoints ==========

        /// <summary>
        /// Confirm vehicle pickup by admin/staff.
        /// Updates order status from ContractGenerated to InProgress.
        /// Records vehicle condition (odometer, battery level) and staff information.
        /// Sends real-time SignalR notification to customer.
        /// </summary>
        [HttpPost("confirm-pickup")]
        [Authorize(Policy = "AdminOrStaff")]
        public async Task<IActionResult> ConfirmPickup([FromBody] ConfirmPickupRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var order = await _orderService.ConfirmPickupAsync(request);

                return Ok(new
                {
                    success = true,
                    message = "Vehicle pickup confirmed successfully. Customer has been notified.",
                    data = new
                    {
                        orderId = order.OrderId,
                        status = order.Status,
                        actualPickupTime = order.ActualPickupTime,
                        odometerReading = order.PickupOdometerReading,
                        batteryLevel = order.PickupBatteryLevel,
                        staffId = order.HandedOverByStaffId
                    }
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while confirming pickup",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Confirm vehicle return by admin/staff.
        /// Updates order status from InProgress to Returned.
        /// Records vehicle condition, calculates late fees if applicable.
        /// Sends real-time SignalR notification to customer.
        /// </summary>
        [HttpPost("confirm-return")]
        [Authorize(Policy = "AdminOrStaff")]
        public async Task<IActionResult> ConfirmReturn([FromBody] ConfirmReturnRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var order = await _orderService.ConfirmReturnAsync(request);

                return Ok(new
                {
                    success = true,
                    message = order.IsLateReturn
                        ? $"Vehicle return confirmed. Late return detected - {order.LateReturnHours} hours late. Late fee: {order.LateFee:N0} VND"
                        : "Vehicle return confirmed successfully. Vehicle is ready for inspection.",
                    data = new
                    {
                        orderId = order.OrderId,
                        status = order.Status,
                        actualReturnTime = order.ActualReturnTime,
                        odometerReading = order.ReturnOdometerReading,
                        batteryLevel = order.ReturnBatteryLevel,
                        staffId = order.ReceivedByStaffId,
                        isLateReturn = order.IsLateReturn,
                        lateReturnHours = order.LateReturnHours,
                        lateFee = order.LateFee
                    }
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while confirming return",
                    error = ex.Message
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
