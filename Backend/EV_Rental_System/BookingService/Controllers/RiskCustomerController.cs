using BookingService.DTOs;
using BookingService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.Controllers
{
    [ApiController]
    [Route("api/risk-customers")]
    [Authorize(Roles = "Admin")] // Admin only
    public class RiskCustomerController : ControllerBase
    {
        private readonly IRiskCustomerService _riskCustomerService;
        private readonly ILogger<RiskCustomerController> _logger;

        public RiskCustomerController(
            IRiskCustomerService riskCustomerService,
            ILogger<RiskCustomerController> logger)
        {
            _riskCustomerService = riskCustomerService;
            _logger = logger;
        }

        /// <summary>
        /// Get all risk customers with calculated risk scores
        /// </summary>
        /// <param name="riskLevel">Filter by risk level: Low, Medium, High, Critical</param>
        /// <param name="minRiskScore">Minimum risk score (0-100)</param>
        [HttpGet]
        public async Task<IActionResult> GetRiskCustomers([FromQuery] string? riskLevel = null, [FromQuery] int? minRiskScore = null)
        {
            try
            {
                var riskCustomers = await _riskCustomerService.GetRiskCustomersAsync(riskLevel, minRiskScore);
                return Ok(new { Success = true, Data = riskCustomers });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting risk customers");
                return StatusCode(500, new { Success = false, Message = "Lỗi hệ thống khi lấy danh sách khách hàng rủi ro." });
            }
        }

        /// <summary>
        /// Get detailed risk profile for a specific user
        /// </summary>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserRiskProfile(int userId)
        {
            try
            {
                var profile = await _riskCustomerService.GetUserRiskProfileAsync(userId);
                if (profile == null)
                {
                    return NotFound(new { Success = false, Message = $"Không tìm thấy thông tin rủi ro cho user {userId}." });
                }
                return Ok(new { Success = true, Data = profile });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting risk profile for User {UserId}", userId);
                return StatusCode(500, new { Success = false, Message = "Lỗi hệ thống khi lấy thông tin rủi ro." });
            }
        }

        /// <summary>
        /// Calculate risk score for a specific user
        /// </summary>
        [HttpPost("{userId}/calculate")]
        public async Task<IActionResult> CalculateUserRisk(int userId)
        {
            try
            {
                var riskCustomer = await _riskCustomerService.CalculateUserRiskAsync(userId);
                if (riskCustomer == null)
                {
                    return NotFound(new { Success = false, Message = $"Không thể tính toán rủi ro cho user {userId}." });
                }
                return Ok(new { Success = true, Data = riskCustomer });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating risk for User {UserId}", userId);
                return StatusCode(500, new { Success = false, Message = "Lỗi hệ thống khi tính toán rủi ro." });
            }
        }
    }
}

