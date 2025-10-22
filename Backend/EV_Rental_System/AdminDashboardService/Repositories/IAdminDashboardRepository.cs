using AdminDashboardService.DTOs;

namespace AdminDashboardService.Repositories
{
    public interface IAdminDashboardRepository
    {
        // Main dashboard summary
        Task<DashboardSummaryDTO> GetDashboardSummaryAsync();

        // Station statistics
        Task<List<StationStatisticDTO>> GetStationStatisticsAsync();

        // Revenue analytics
        Task<List<RevenueByMonthDTO>> GetRevenueByMonthAsync(int year);

        // Vehicle analytics
        Task<List<VehicleUsageDTO>> GetTopUsedVehiclesAsync(int top = 10);

        // User growth analytics
        Task<UserGrowthStatisticsDTO> GetUserGrowthStatisticsAsync();
    }
}