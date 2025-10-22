using BookingService.DTOs;
using BookingService.DTOs.Fees;
using BookingService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeeController : ControllerBase
    {
        private readonly IFeeCalculationService _feeService;

        public FeeController(IFeeCalculationService feeService)
        {
            _feeService = feeService;
        }

        /// <summary>
        /// Calculate all applicable fees for an order after rental completion
        /// </summary>
        [HttpPost("calculate")]
        [Authorize(Roles = "Employee,Admin")]
        public async Task<IActionResult> CalculateFees([FromBody] FeeCalculationRequest request)
        {
            try
            {
                var result = await _feeService.CalculateFeesAsync(request);
                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Fee calculation failed: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Manually add a fee to an order
        /// </summary>
        [HttpPost("add")]
        [Authorize(Roles = "Employee,Admin")]
        public async Task<IActionResult> AddFee([FromBody] AddFeeRequest request)
        {
            try
            {
                // Get user ID from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    request.CalculatedBy = userId;
                }

                var fee = await _feeService.AddFeeAsync(request);
                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Fee added successfully",
                    Data = fee
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Failed to add fee: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get all fees for a specific order
        /// </summary>
        [HttpGet("order/{orderId}")]
        [Authorize(Roles = "Employee,Admin,Member")]
        public async Task<IActionResult> GetFeesByOrderId(int orderId)
        {
            try
            {
                var fees = await _feeService.GetFeesByOrderIdAsync(orderId);
                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = $"Found {fees.Count} fee(s)",
                    Data = fees
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Failed to retrieve fees: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get a specific fee by ID
        /// </summary>
        [HttpGet("{feeId}")]
        [Authorize(Roles = "Employee,Admin,Member")]
        public async Task<IActionResult> GetFeeById(int feeId)
        {
            try
            {
                var fee = await _feeService.GetFeeByIdAsync(feeId);
                if (fee == null)
                {
                    return NotFound(new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Fee not found"
                    });
                }

                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Fee retrieved successfully",
                    Data = fee
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Failed to retrieve fee: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get total fees amount for an order
        /// </summary>
        [HttpGet("order/{orderId}/total")]
        [Authorize(Roles = "Employee,Admin,Member")]
        public async Task<IActionResult> GetTotalFees(int orderId)
        {
            try
            {
                var total = await _feeService.GetTotalFeesAsync(orderId);
                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Total fees calculated",
                    Data = new { OrderId = orderId, TotalFees = total }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Failed to calculate total fees: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Mark a fee as paid
        /// </summary>
        [HttpPut("{feeId}/mark-paid")]
        [Authorize(Roles = "Employee,Admin")]
        public async Task<IActionResult> MarkFeeAsPaid(int feeId)
        {
            try
            {
                var success = await _feeService.MarkFeeAsPaidAsync(feeId);
                if (!success)
                {
                    return NotFound(new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Fee not found"
                    });
                }

                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Fee marked as paid"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Failed to update fee: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Delete a fee (only unpaid fees can be deleted)
        /// </summary>
        [HttpDelete("{feeId}")]
        [Authorize(Roles = "Employee,Admin")]
        public async Task<IActionResult> DeleteFee(int feeId)
        {
            try
            {
                var success = await _feeService.DeleteFeeAsync(feeId);
                if (!success)
                {
                    return BadRequest(new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Fee not found or already paid (cannot delete paid fees)"
                    });
                }

                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Fee deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Failed to delete fee: {ex.Message}"
                });
            }
        }
    }
}
