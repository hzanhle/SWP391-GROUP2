using UserService.Models;
using UserService.Repositories;

namespace UserService.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task AddNotification(Notification notification)
        {
            await _notificationRepository.AddNotification(notification);
        }

        public async Task<List<Notification>> GetAllByUserId(int userId)
        {
            return await _notificationRepository.GetAllByUserId(userId);
        }

        public async Task RemoveNotificationByUserId(int userId)
        {
            await _notificationRepository.RemoveNotificationByUserId(userId);
        }

        public async Task UpdateNotification(Notification notification)
        {
            try
            {
                var existingNotifications = await _notificationRepository.GetNotification(notification.UserId);
                if (existingNotifications != null)
                {
                    existingNotifications.Title = notification.Title;
                    existingNotifications.Message = notification.Message;
                    existingNotifications.Created = DateTime.Now;
                }
                await _notificationRepository.UpdateNotification(existingNotifications);
            } catch (Exception ex)
            {
                throw new Exception($"Error updating notification: {ex.Message}");
            }
        }
    }
}
