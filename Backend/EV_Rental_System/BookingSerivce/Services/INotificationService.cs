using BookingSerivce.DTOs;
using BookingSerivce.Models;

namespace BookingSerivce.Services
{
    public interface INotificationService
    {
        Task AddNotification(NotificationRequest request );
        Task DeleteNotification(int userId);
        Task UpdateNotification(NotificationRequest request);
        Task<List<Notification>> GetAllNotifications();
        Task<List<Notification>> GetNotificationsByUserId(int userId);
    }
}
