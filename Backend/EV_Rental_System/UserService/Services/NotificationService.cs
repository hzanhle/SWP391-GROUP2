using Microsoft.Extensions.Logging;
using UserService.Models;
using UserService.Repositories;

namespace UserService.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(INotificationRepository notificationRepository, ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _logger = logger;
        }

        public async Task AddNotification(Notification notification)
        {
            try
            {
                await _notificationRepository.AddNotification(notification);
                _logger.LogInformation("✅ Added notification for user {UserId}: {Title}", notification.UserId, notification.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error adding notification for user {UserId}", notification.UserId);
                throw;
            }
        }

        public async Task<List<Notification>> GetAllByUserId(int userId)
        {
            try
            {
                var notifications = await _notificationRepository.GetAllByUserId(userId);
                _logger.LogInformation("📥 Retrieved {Count} notifications for user {UserId}", notifications.Count, userId);
                return notifications;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error retrieving notifications for user {UserId}", userId);
                throw;
            }
        }

        public async Task RemoveNotificationByUserId(int userId)
        {
            try
            {
                await _notificationRepository.RemoveNotificationByUserId(userId);
                _logger.LogInformation("🗑️ Removed notifications for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error removing notifications for user {UserId}", userId);
                throw;
            }
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

                    await _notificationRepository.UpdateNotification(existingNotifications);
                    _logger.LogInformation("✏️ Updated notification for user {UserId}: {Title}", notification.UserId, notification.Title);
                }
                else
                {
                    _logger.LogWarning("⚠️ No notification found to update for user {UserId}", notification.UserId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating notification for user {UserId}", notification.UserId);
                throw;
            }
        }
    }
}
