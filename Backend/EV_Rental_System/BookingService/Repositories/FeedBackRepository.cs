using BookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Repositories
{
    public class FeedBackRepository : IFeedBackRepository
    {
        private readonly MyDbContext _context;

        public FeedBackRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(Feedback feedback)
        {
            await _context.Feedbacks.AddAsync(feedback);
            await _context.SaveChangesAsync();
        }

        public async Task<Feedback?> GetByFeedBackIdAsync(int feedBackId)
        {
            return await _context.Feedbacks
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.FeedBackId == feedBackId);
        }

        public async Task<Feedback?> GetByOrderIdAsync(int orderId)
        {
            return await _context.Feedbacks
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.OrderId == orderId);
        }

        public async Task<IEnumerable<Feedback>> GetByUserIdAsync(int userId)
        {
            return await _context.Feedbacks
                .AsNoTracking()
                .Where(f => f.UserId == userId)
                .ToListAsync();
        }

        public async Task UpdateAsync(Feedback feedback)
        {
            _context.Feedbacks.Update(feedback);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Feedback feedback)
        {
            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();
        }
    }
}
