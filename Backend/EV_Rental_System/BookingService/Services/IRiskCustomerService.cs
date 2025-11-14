using BookingService.DTOs;

namespace BookingService.Services
{
    public interface IRiskCustomerService
    {
        /// <summary>
        /// Get all risk customers with calculated risk scores
        /// </summary>
        Task<List<RiskCustomerDTO>> GetRiskCustomersAsync(string? riskLevel = null, int? minRiskScore = null);

        /// <summary>
        /// Get detailed risk profile for a specific user
        /// </summary>
        Task<UserRiskProfileDTO?> GetUserRiskProfileAsync(int userId);

        /// <summary>
        /// Calculate risk score for a specific user
        /// </summary>
        Task<RiskCustomerDTO?> CalculateUserRiskAsync(int userId);
    }
}

