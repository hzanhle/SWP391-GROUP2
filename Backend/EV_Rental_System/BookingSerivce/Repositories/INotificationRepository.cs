using BookingSerivce.Models;
using BookingService.Models;

namespace BookingSerivce.Repositories
{
    public interface INotificationRepository
    {
        Task AddNotification(Notification notification);
        Task<List<Notification>> GetAllNotifications();
        Task<List<Notification>> GetNotificationsByUserId(int userId);
        Task DeleteNotification(int userId);
        Task UpdateNotification(Notification notification);
    }
}
