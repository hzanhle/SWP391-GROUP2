using UserService.Models;

namespace UserService.Services
{
    public interface INotificationService
    {
        Task AddNotification(Notification notification);
        Task RemoveNotificationByUserId(int userId);
        Task<List<Notification>> GetAllByUserId(int userId);
        Task UpdateNotification(Notification notification);
    }
}
