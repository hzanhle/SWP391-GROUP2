using AdminDashboardService.DTOs;

namespace AdminDashboardService.Services
{
    public interface IRentalAnalyticsService
    {
        /// <summary>
        /// Get rental statistics for a specific user
        /// </summary>
        Task<UserRentalStatsResponse> GetUserRentalStatsAsync(int userId);

        /// <summary>
        /// Get system-wide rental analytics (all users)
        /// </summary>
        Task<SystemRentalAnalyticsResponse> GetSystemRentalAnalyticsAsync();

        /// <summary>
        /// Get peak rental hours analysis
        /// </summary>
        Task<PeakHoursResponse> GetPeakRentalHoursAsync();
    }
}
