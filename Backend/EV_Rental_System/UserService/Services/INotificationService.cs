using UserService.Models;

namespace UserService.Services
{
    public interface INotificationService
    {
        Task AddNotification(Notification notification);
        Task RemoveNotificationByUserId(int userId); // Xóa tất cả notifications của user
        Task<List<Notification>> GetAllByUserId(int userId);
        Task UpdateNotification(Notification notification);
    }
}
