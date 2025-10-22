using AdminDashboardService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AdminDashboardService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] //Chỉ có Admin mới có quyền 
    public class DashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IAdminDashboardService dashboardService,
            ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy tổng quan dashboard - bao gồm tất cả thống kê chính
        /// </summary>
        /// <returns>Dashboard summary với các metrics chính</returns>
        [HttpGet("summary")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            _logger.LogInformation("GET api/dashboard/summary - Request received");
            try
            {
                var summary = await _dashboardService.GetDashboardSummaryAsync();
                _logger.LogInformation("Tóm tắt bảng điều khiển đã được truy xuất thành công");
                return Ok(new
                {
                    success = true,
                    data = summary,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi truy xuất tóm tắt bảng điều khiển");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi lấy dữ liệu dashboard",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy thống kê chi tiết các trạm
        /// </summary>
        /// <returns>Danh sách thống kê từng trạm</returns>
        [HttpGet("stations")]
        public async Task<IActionResult> GetStationStatistics()
        {
            _logger.LogInformation("GET api/dashboard/stations - Request received");
            try
            {
                var stats = await _dashboardService.GetStationStatisticsAsync();
                return Ok(new
                {
                    success = true,
                    data = stats,
                    total = stats.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi truy xuất số liệu thống kê trạm");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi lấy thống kê trạm",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy doanh thu theo tháng trong năm
        /// </summary>
        /// <param name="year">Năm cần thống kê (mặc định: năm hiện tại)</param>
        /// <returns>Doanh thu từng tháng</returns>
        [HttpGet("revenue/monthly")]
        public async Task<IActionResult> GetRevenueByMonth([FromQuery] int? year)
        {
            var targetYear = year ?? DateTime.UtcNow.Year;
            _logger.LogInformation("GET api/dashboard/revenue/monthly?year={Year} - Request received", targetYear);

            try
            {
                var revenue = await _dashboardService.GetRevenueByMonthAsync(targetYear);
                return Ok(new
                {
                    success = true,
                    year = targetYear,
                    data = revenue,
                    totalRevenue = revenue.Sum(r => r.TotalRevenue)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi truy xuất doanh thu theo tháng");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi lấy dữ liệu doanh thu",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy danh sách xe được thuê nhiều nhất
        /// </summary>
        /// <param name="top">Số lượng xe muốn lấy (mặc định: 10)</param>
        /// <returns>Top xe được sử dụng nhiều nhất</returns>
        [HttpGet("vehicles/top-used")]
        public async Task<IActionResult> GetTopUsedVehicles([FromQuery] int top = 10)
        {
            _logger.LogInformation("GET api/dashboard/vehicles/top-used?top={Top} - Request received", top);

            if (top <= 0 || top > 100)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Tham số 'top' phải từ 1 đến 100"
                });
            }

            try
            {
                var vehicles = await _dashboardService.GetTopUsedVehiclesAsync(top);
                return Ok(new
                {
                    success = true,
                    data = vehicles,
                    count = vehicles.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã xảy ra lỗi khi lấy danh sách những xe đứng đầu.");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi lấy dữ liệu xe",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy thống kê tăng trưởng người dùng
        /// </summary>
        /// <returns>Thống kê tăng trưởng user</returns>
        [HttpGet("users/growth")]
        public async Task<IActionResult> GetUserGrowthStatistics()
        {
            _logger.LogInformation("GET api/dashboard/users/growth - Request received");

            try
            {
                var stats = await _dashboardService.GetUserGrowthStatisticsAsync();
                return Ok(new
                {
                    success = true,
                    data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi truy xuất số liệu thống kê về mức tăng trưởng của người dùng");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi lấy thống kê người dùng",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous] // Cho phép tất cả người dùng có thể theo dõi
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                status = "healthy",
                service = "AdminDashboardService",
                timestamp = DateTime.UtcNow
            });
        }
    }
}