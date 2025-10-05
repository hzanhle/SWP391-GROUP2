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
    }
}
