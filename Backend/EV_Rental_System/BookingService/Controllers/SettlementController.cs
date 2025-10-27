using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookingService.DTOs;
using BookingService.Services;

namespace BookingService.Controllers
{
    [ApiController]
    [Route("api/settlement")]
    [Authorize] // Require authentication for all settlement endpoints
    public class SettlementController : ControllerBase
    {
        private readonly ISettlementService _settlementService;
        private readonly ILogger<SettlementController> _logger;

        public SettlementController(
            ISettlementService settlementService,
            ILogger<SettlementController> logger)
        {
            _settlementService = settlementService;
            _logger = logger;
        }

        /// <summary>
        /// Calculate settlement preview (not saved to database)
        /// Staff can preview charges before creating settlement
        /// </summary>
        [HttpPost("calculate/{orderId}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> CalculateSettlement(int orderId, [FromBody] SettlementCalculationRequest request)
        {
            try
            {
                if (orderId != request.OrderId)
                    return BadRequest(new { Message = "Order ID mismatch" });

                var result = await _settlementService.CalculateSettlementAsync(orderId, request.ActualReturnTime);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation calculating settlement for Order {OrderId}", orderId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating settlement for Order {OrderId}", orderId);
                return StatusCode(500, new { Message = "Lỗi khi tính toán settlement" });
            }
        }

        /// <summary>
        /// Create settlement for an order
        /// Creates and saves settlement with overtime calculation
        /// </summary>
        [HttpPost("{orderId}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> CreateSettlement(int orderId, [FromBody] SettlementCalculationRequest request)
        {
            try
            {
                if (orderId != request.OrderId)
                    return BadRequest(new { Message = "Order ID mismatch" });

                var result = await _settlementService.CreateSettlementAsync(orderId, request.ActualReturnTime);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation creating settlement for Order {OrderId}", orderId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating settlement for Order {OrderId}", orderId);
                return StatusCode(500, new { Message = "Lỗi khi tạo settlement" });
            }
        }

        /// <summary>
        /// Add damage charge to an existing settlement
        /// Staff can add damage costs before finalizing
        /// </summary>
        [HttpPost("{orderId}/damage")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> AddDamageCharge(int orderId, [FromBody] AddDamageChargeRequest request)
        {
            try
            {
                var result = await _settlementService.AddDamageChargeAsync(
                    orderId,
                    request.Amount,
                    request.Description);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation adding damage charge for Order {OrderId}", orderId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding damage charge for Order {OrderId}", orderId);
                return StatusCode(500, new { Message = "Lỗi khi thêm phí hư hỏng" });
            }
        }

        /// <summary>
        /// Finalize settlement
        /// Locks in the settlement and triggers invoice generation
        /// </summary>
        [HttpPost("{orderId}/finalize")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> FinalizeSettlement(int orderId)
        {
            try
            {
                var result = await _settlementService.FinalizeSettlementAsync(orderId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation finalizing settlement for Order {OrderId}", orderId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finalizing settlement for Order {OrderId}", orderId);
                return StatusCode(500, new { Message = "Lỗi khi xác nhận settlement" });
            }
        }

        /// <summary>
        /// Get settlement details by order ID
        /// Members can view their own settlement, Staff can view any
        /// </summary>
        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetSettlementByOrderId(int orderId)
        {
            try
            {
                var result = await _settlementService.GetSettlementByOrderIdAsync(orderId);
                if (result == null)
                    return NotFound(new { Message = $"Settlement not found for Order {orderId}" });

                // TODO: Add authorization check - Members can only view their own settlements
                // var userId = GetUserIdFromClaims();
                // if (userRole != "Admin" && userRole != "Employee" && result.UserId != userId)
                //     return Forbid();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting settlement for Order {OrderId}", orderId);
                return StatusCode(500, new { Message = "Lỗi khi lấy thông tin settlement" });
            }
        }

        /// <summary>
        /// Get settlement details by settlement ID
        /// </summary>
        [HttpGet("{settlementId}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetSettlementById(int settlementId)
        {
            try
            {
                var result = await _settlementService.GetSettlementByIdAsync(settlementId);
                if (result == null)
                    return NotFound(new { Message = $"Settlement {settlementId} not found" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Settlement {SettlementId}", settlementId);
                return StatusCode(500, new { Message = "Lỗi khi lấy thông tin settlement" });
            }
        }
    }
}
