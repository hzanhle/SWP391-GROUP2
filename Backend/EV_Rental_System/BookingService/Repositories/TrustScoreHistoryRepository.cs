using BookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Repositories
{
    public class TrustScoreHistoryRepository : ITrustScoreHistoryRepository
    {
        private readonly MyDbContext _context;

        public TrustScoreHistoryRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<TrustScoreHistory> CreateAsync(TrustScoreHistory history)
        {
            _context.TrustScoreHistories.Add(history);
            await _context.SaveChangesAsync();
            return history;
        }

        public async Task<List<TrustScoreHistory>> GetByUserIdAsync(int userId)
        {
            return await _context.TrustScoreHistories
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<TrustScoreHistory>> GetByOrderIdAsync(int orderId)
        {
            return await _context.TrustScoreHistories
                .Where(h => h.OrderId == orderId)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<TrustScoreHistory>> GetRecentByUserIdAsync(int userId, int count = 10)
        {
            return await _context.TrustScoreHistories
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<TrustScoreHistory>> GetAllAsync()
        {
            return await _context.TrustScoreHistories
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();
        }
    }
}
