using AdminDashboardService.DTOs;
using AdminDashboardService.Repositories;

namespace AdminDashboardService.Services
{
    public interface IAdminDashboardService
    {
        Task<DashboardSummaryDTO> GetDashboardSummaryAsync();
        Task<List<StationStatisticDTO>> GetStationStatisticsAsync();
        Task<List<RevenueByMonthDTO>> GetRevenueByMonthAsync(int year);
        Task<List<VehicleUsageDTO>> GetTopUsedVehiclesAsync(int top = 10);
        Task<UserGrowthStatisticsDTO> GetUserGrowthStatisticsAsync();
    }
}
