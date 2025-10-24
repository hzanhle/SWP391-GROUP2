using BookingService.Models;

namespace BookingService.Repositories
{
    public interface IFeedBackRepository
    {
        Task CreateAsync(Feedback feedback);
        Task<Feedback?> GetByOrderIdAsync(int orderId);
        Task<Feedback?> GetByFeedBackIdAsync(int id);
        Task<IEnumerable<Feedback>> GetByUserIdAsync(int userId);
        Task UpdateAsync(Feedback feedback);
        Task DeleteAsync(Feedback feedBack);
    }
}
