using BookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Repositories
{
    public class TrustScoreRepository : ITrustScoreRepository
    {
        private readonly MyDbContext _context;

        public TrustScoreRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<TrustScore> CreateAsync(TrustScore trustScore)
        {
            _context.TrustScores.Add(trustScore);
            await _context.SaveChangesAsync();
            return trustScore;
        }

        public async Task<TrustScore?> GetByIdAsync(int trustScoreId)
        {
            return await _context.TrustScores.FindAsync(trustScoreId);
        }

        public async Task<TrustScore?> GetByUserIdAsync(int userId)
        {
            return await _context.TrustScores
                .FirstOrDefaultAsync(ts => ts.UserId == userId);
        }

        public async Task<TrustScore> UpdateAsync(TrustScore trustScore)
        {
            _context.TrustScores.Update(trustScore);
            await _context.SaveChangesAsync();
            return trustScore;
        }

        public async Task<bool> DeleteAsync(int trustScoreId)
        {
            var trustScore = await _context.TrustScores.FindAsync(trustScoreId);
            if (trustScore == null) return false;

            _context.TrustScores.Remove(trustScore);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteByUserIdAsync(int userId)
        {
            var trustScore = await _context.TrustScores
                .FirstOrDefaultAsync(ts => ts.UserId == userId);

            if (trustScore == null) return false;

            _context.TrustScores.Remove(trustScore);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateScoreAsync(TrustScore trustScore)
        {
            var existingTrustScore = await _context.TrustScores
                .FirstOrDefaultAsync(ts => ts.UserId == trustScore.UserId);

            if (existingTrustScore == null) return false;

            existingTrustScore.Score = trustScore.Score;
            existingTrustScore.OrderId = trustScore.OrderId;
            existingTrustScore.CreatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<TrustScore>> GetTopScoresAsync()
        {
            return await _context.TrustScores
                .OrderByDescending(ts => ts.Score)
                .ToListAsync();
        }

        public async Task<List<TrustScore>> GetScoresByRangeAsync(int minScore, int maxScore)
        {
            return await _context.TrustScores
                .Where(ts => ts.Score >= minScore && ts.Score <= maxScore)
                .OrderByDescending(ts => ts.Score)
                .ToListAsync();
        }

        public async Task<double> GetAverageScoreAsync()
        {
            if (!await _context.TrustScores.AnyAsync())
                return 0;

            return await _context.TrustScores.AverageAsync(ts => ts.Score);
        }

        public async Task<bool> ExistsByUserIdAsync(int userId)
        {
            return await _context.TrustScores
                .AnyAsync(ts => ts.UserId == userId);
        }
    }
}
