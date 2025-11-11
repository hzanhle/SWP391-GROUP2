using BookingService.Models;

namespace BookingService.Repositories
{
    public interface ITrustScoreHistoryRepository
    {
        /// <summary>
        /// Add a new trust score history entry
        /// </summary>
        Task<TrustScoreHistory> CreateAsync(TrustScoreHistory history);

        /// <summary>
        /// Get all history entries for a specific user
        /// </summary>
        Task<List<TrustScoreHistory>> GetByUserIdAsync(int userId);

        /// <summary>
        /// Get history entries for a specific order
        /// </summary>
        Task<List<TrustScoreHistory>> GetByOrderIdAsync(int orderId);

        /// <summary>
        /// Get recent history entries (last N changes) for a user
        /// </summary>
        Task<List<TrustScoreHistory>> GetRecentByUserIdAsync(int userId, int count = 10);

        /// <summary>
        /// Get all history entries (for admin purposes)
        /// </summary>
        Task<List<TrustScoreHistory>> GetAllAsync();
    }
}
