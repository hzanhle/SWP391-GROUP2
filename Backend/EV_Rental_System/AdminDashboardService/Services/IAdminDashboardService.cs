using AdminDashboardService.DTOs;
using System.Threading.Tasks;

namespace AdminDashboardService.Services
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardDTO> GetDashboardMetricsAsync();
    }
}
