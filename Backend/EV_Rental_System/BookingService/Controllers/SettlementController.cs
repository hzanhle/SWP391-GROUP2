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

        /// <summary>
        /// Process automatic refund via VNPay Refund API
        /// This will attempt to refund the deposit automatically through VNPay
        /// If automatic refund fails, status will be set to AwaitingManualProof
        /// </summary>
        [HttpPost("{orderId}/refund/automatic")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ProcessAutomaticRefund(int orderId)
        {
            try
            {
                // Get admin ID from JWT token
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int adminId))
                {
                    return Unauthorized(new { Message = "Không thể xác thực admin." });
                }

                var result = await _settlementService.ProcessAutomaticRefundAsync(orderId, adminId);

                // Return appropriate message based on refund status
                var message = result.RefundStatus switch
                {
                    "Processed" => "Hoàn tiền tự động thành công qua VNPay API",
                    "AwaitingManualProof" => "Hoàn tiền tự động thất bại. Vui lòng xử lý hoàn tiền thủ công và upload minh chứng.",
                    "NotRequired" => "Không cần hoàn tiền cho đơn hàng này",
                    _ => "Đã cập nhật trạng thái hoàn tiền"
                };

                return Ok(new
                {
                    Message = message,
                    Settlement = result
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation processing automatic refund for Order {OrderId}", orderId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing automatic refund for Order {OrderId}", orderId);
                return StatusCode(500, new { Message = "Lỗi khi xử lý hoàn tiền tự động" });
            }
        }

        /// <summary>
        /// Mark refund as processed with proof document (manual refund via VNPay portal)
        /// Admin must upload proof document (screenshot, receipt, etc.) as evidence of manual refund
        /// </summary>
        [HttpPost("{orderId}/refund/manual")]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> MarkRefundAsProcessed(
            int orderId,
            [FromForm] ManualRefundRequest request)
        {
            try
            {
                // Get admin ID from JWT token
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int adminId))
                {
                    return Unauthorized(new { Message = "Không thể xác thực admin." });
                }

                // Validate proof document
                if (request.ProofDocument == null || request.ProofDocument.Length == 0)
                {
                    return BadRequest(new { Message = "Vui lòng upload minh chứng hoàn tiền (ảnh chụp màn hình VNPay, biên lai, v.v.)" });
                }

                var result = await _settlementService.MarkRefundAsProcessedAsync(orderId, adminId, request.ProofDocument, request.Notes);
                return Ok(new
                {
                    Message = "Đã cập nhật trạng thái hoàn tiền thành công với minh chứng",
                    Settlement = result
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for manual refund for Order {OrderId}", orderId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation marking refund as processed for Order {OrderId}", orderId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking refund as processed for Order {OrderId}", orderId);
                return StatusCode(500, new { Message = "Lỗi khi cập nhật trạng thái hoàn tiền" });
            }
        }

        /// <summary>
        /// Mark refund as failed
        /// </summary>
        [HttpPost("{orderId}/refund/failed")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkRefundAsFailed(int orderId, [FromBody] RefundProcessRequest? request = null)
        {
            try
            {
                // Get admin ID from JWT token
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int adminId))
                {
                    return Unauthorized(new { Message = "Không thể xác thực admin." });
                }

                var result = await _settlementService.MarkRefundAsFailedAsync(orderId, adminId, request?.Notes);
                return Ok(new
                {
                    Message = "Đã đánh dấu hoàn tiền thất bại",
                    Settlement = result
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation marking refund as failed for Order {OrderId}", orderId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking refund as failed for Order {OrderId}", orderId);
                return StatusCode(500, new { Message = "Lỗi khi đánh dấu hoàn tiền thất bại" });
            }
        }

        // =====================================================================
        // ADDITIONAL PAYMENT ENDPOINTS (for when deposit is insufficient)
        // =====================================================================

        /// <summary>
        /// Get or regenerate additional payment URL for a settlement
        /// </summary>
        [HttpPost("{orderId}/payment")]
        [Authorize(Roles = "Member,Admin")]
        public async Task<IActionResult> GetAdditionalPaymentUrl(int orderId)
        {
            try
            {
                _logger.LogInformation("Getting additional payment URL for Order {OrderId}", orderId);

                var paymentUrl = await _settlementService.GetAdditionalPaymentUrlAsync(orderId);

                return Ok(new
                {
                    Message = "Additional payment URL generated successfully",
                    PaymentUrl = paymentUrl
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation getting payment URL for Order {OrderId}", orderId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting additional payment URL for Order {OrderId}", orderId);
                return StatusCode(500, new { Message = "Lỗi khi tạo link thanh toán bổ sung" });
            }
        }

        /// <summary>
        /// VNPay callback endpoint for additional payment processing
        /// </summary>
        [HttpGet("payment-return")]
        [AllowAnonymous]
        public async Task<IActionResult> ProcessAdditionalPaymentCallback()
        {
            try
            {
                var queryParams = Request.Query;
                var txnRef = queryParams["vnp_TxnRef"].ToString();

                _logger.LogInformation("Processing additional payment callback for TxnRef: {TxnRef}", txnRef);

                var result = await _settlementService.ProcessAdditionalPaymentCallbackAsync(txnRef, queryParams);

                var responseCode = queryParams["vnp_ResponseCode"].ToString();
                if (responseCode == "00")
                {
                    return Ok(new
                    {
                        Message = "Additional payment completed successfully",
                        Settlement = result
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        Message = "Additional payment failed",
                        ResponseCode = responseCode,
                        Settlement = result
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing additional payment callback");
                return StatusCode(500, new { Message = "Lỗi khi xử lý callback thanh toán" });
            }
        }

        /// <summary>
        /// Check additional payment status for a settlement
        /// </summary>
        [HttpGet("{orderId}/payment-status")]
        [Authorize(Roles = "Member,Admin,Employee")]
        public async Task<IActionResult> CheckAdditionalPaymentStatus(int orderId)
        {
            try
            {
                _logger.LogInformation("Checking additional payment status for Order {OrderId}", orderId);

                var status = await _settlementService.CheckAdditionalPaymentStatusAsync(orderId);

                return Ok(new
                {
                    OrderId = orderId,
                    AdditionalPaymentStatus = status.ToString()
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation checking payment status for Order {OrderId}", orderId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking additional payment status for Order {OrderId}", orderId);
                return StatusCode(500, new { Message = "Lỗi khi kiểm tra trạng thái thanh toán" });
            }
        }
    }
}
