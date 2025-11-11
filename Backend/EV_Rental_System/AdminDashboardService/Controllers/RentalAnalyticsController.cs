using AdminDashboardService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminDashboardService.Controllers
{
    [ApiController]
    [Route("api/analytics")]
    public class RentalAnalyticsController : ControllerBase
    {
        private readonly IRentalAnalyticsService _analyticsService;
        private readonly ILogger<RentalAnalyticsController> _logger;

        public RentalAnalyticsController(
            IRentalAnalyticsService analyticsService,
            ILogger<RentalAnalyticsController> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        /// <summary>
        /// Get rental statistics for a specific user
        /// </summary>
        /// <param name="userId">User ID to get stats for</param>
        /// <returns>User rental statistics</returns>
        [HttpGet("user/{userId}/rental-stats")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetUserRentalStats(int userId)
        {
            try
            {
                var stats = await _analyticsService.GetUserRentalStatsAsync(userId);

                if (stats == null)
                {
                    return NotFound(new { message = $"No rental data found for user {userId}" });
                }

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rental stats for user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while retrieving user rental statistics" });
            }
        }

        /// <summary>
        /// Get system-wide rental analytics (all users combined)
        /// </summary>
        /// <returns>System-wide rental analytics</returns>
        [HttpGet("rentals/system-wide")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSystemRentalAnalytics()
        {
            try
            {
                var analytics = await _analyticsService.GetSystemRentalAnalyticsAsync();

                if (analytics == null)
                {
                    return NotFound(new { message = "No rental data found in the system" });
                }

                return Ok(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system-wide rental analytics");
                return StatusCode(500, new { message = "An error occurred while retrieving system rental analytics" });
            }
        }

        /// <summary>
        /// Get peak rental hours analysis (0-23 hour breakdown)
        /// </summary>
        /// <returns>Peak hours analysis with top/low hours</returns>
        [HttpGet("rentals/peak-hours")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPeakRentalHours()
        {
            try
            {
                var peakHours = await _analyticsService.GetPeakRentalHoursAsync();

                if (peakHours == null || !peakHours.HourlyStats.Any())
                {
                    return NotFound(new { message = "No rental data found for peak hours analysis" });
                }

                return Ok(peakHours);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting peak rental hours");
                return StatusCode(500, new { message = "An error occurred while retrieving peak rental hours" });
            }
        }
    }
}
