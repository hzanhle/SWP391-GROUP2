

using AdminDashboardService.DTOs;
using AdminDashboardService.Repositories;
using System.Threading.Tasks;

namespace AdminDashboardService.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IAdminDashboardRepository _dashboardRepository;

        // Constructor được inject IAdminDashboardRepository
        public AdminDashboardService(IAdminDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        // Phương thức này phải trả về đúng kiểu dữ liệu trong DTO mới
        public async Task<AdminDashboardDTO> GetDashboardMetricsAsync()
        {
            // Service chỉ việc gọi đến phương thức tương ứng của Repository
            // Toàn bộ logic query đã được chuyển sang lớp Repository
            return await _dashboardRepository.GetDashboardMetricsAsync();
        }
    }
}