using AdminDashboardService.DTOs;

namespace AdminDashboardService.Repositories
{
    public interface IAdminDashboardRepository
    {
        Task<AdminDashboardDTO> GetDashboardMetricsAsync();
    }
}
