using UserService.Models;

namespace UserService.Repositories
{
    public interface INotificationRepository
    {
        Task AddNotification(Notification notification);
        Task RemoveNotificationByUserId(int userId);
        Task<List<Notification>> GetAllByUserId(int userId);
        Task<Notification> GetNotification(int notificationId);
        Task UpdateNotification(Notification notification);
    }
}
