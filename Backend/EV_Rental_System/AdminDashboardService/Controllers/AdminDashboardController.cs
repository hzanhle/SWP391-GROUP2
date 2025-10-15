// File: AdminDashboardService/Controllers/DashboardController.cs

using AdminDashboardService.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")] // URL cơ bản sẽ là /api/dashboard
public class DashboardController : ControllerBase
{
    private readonly IAdminDashboardService _dashboardService;

    // Sử dụng Dependency Injection để "tiêm" service vào controller
    public DashboardController(IAdminDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Lấy các chỉ số tổng quan cho Bảng điều khiển của Admin.
    /// </summary>
    /// <returns>Một đối tượng chứa các chỉ số về doanh thu, trạm, người dùng và lượt thuê.</returns>
    [HttpGet("metrics")] // URL đầy đủ sẽ là: GET /api/dashboard/metrics
    public async Task<IActionResult> GetDashboardMetrics()
    {
        // Gọi đến phương thức của service để lấy dữ liệu
        var metricsData = await _dashboardService.GetDashboardMetricsAsync();

        // Trả về dữ liệu với mã trạng thái 200 OK
        return Ok(metricsData);
    }
}