using BookingService.Models;

namespace BookingService.Repositories
{
    public interface ITrustScoreRepository
    {
        Task<TrustScore> CreateAsync(TrustScore trustScore);
        Task<TrustScore?> GetByIdAsync(int trustScoreId);
        Task<TrustScore?> GetByUserIdAsync(int userId);
        Task<TrustScore> UpdateAsync(TrustScore trustScore);
        Task<bool> DeleteAsync(int trustScoreId);
        Task<bool> DeleteByUserIdAsync(int userId);
        Task<bool> UpdateScoreAsync(TrustScore trustScore);
        Task<List<TrustScore>> GetTopScoresAsync();
        Task<List<TrustScore>> GetScoresByRangeAsync(int minScore, int maxScore);
        Task<double> GetAverageScoreAsync();
        Task<bool> ExistsByUserIdAsync(int userId);
    }
}
