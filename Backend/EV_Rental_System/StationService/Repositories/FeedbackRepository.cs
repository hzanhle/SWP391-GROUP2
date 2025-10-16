using Microsoft.EntityFrameworkCore;
using StationService.Models;

namespace StationService.Repositories
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly MyDbContext _context;
        public FeedbackRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<Feedback> AddAsync(Feedback feedback)
        {
            await _context.Feedbacks.AddAsync(feedback);
            await _context.SaveChangesAsync();
            return feedback;
        }

        public async Task DeleteAsync(int id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback != null)
            {
                _context.Feedbacks.Remove(feedback);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Feedback>> GetByStationIdAsync(int stationId)
        {
            return await Task.FromResult(_context.Feedbacks.Where(f => f.StationId == stationId).AsEnumerable());
        }

        public async Task<Feedback?> GetByIdAsync(int id)
        {
            return await _context.Feedbacks.FindAsync(id);
        }
        public async Task UpdateAsync(Feedback feedback)
        {
            _context.Entry(feedback).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

    }
}
