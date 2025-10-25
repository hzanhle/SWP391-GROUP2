using BookingService.Models;

namespace BookingService.Services
{
    public interface IFeedbackService
    {
        Task<Feedback?> GetByFeedbackIdAsync(int id);
        Task<Feedback?> GetByOrderIdAsync(int orderId);
        Task<IEnumerable<Feedback>> GetByUserIdAsync(int userId);
        Task CreateAsync(Feedback feedback);
        Task UpdateAsync(Feedback feedback);
        Task DeleteAsync(int feedbackId);
    }
}
