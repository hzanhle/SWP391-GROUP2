using BookingSerivce.DTOs;
using BookingSerivce.Models;
using BookingSerivce.Repositories;

namespace BookingSerivce.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task AddNotification(NotificationRequest request)
        {
            var notification = new Notification
            {
                Title = request.Title,
                Description = request.Description,
                DataType = request.DataType,
                DataId = request.DataId,
                UserId = request.UserId,
                Created = DateTime.UtcNow,
                StaffId = request.StaffId
            };
            await _notificationRepository.AddNotification(notification);
        }

        public async Task DeleteNotification(int userId)
        {
            await _notificationRepository.DeleteNotification(userId);
        }

        public async Task<List<Notification>> GetAllNotifications()
        {
            return await _notificationRepository.GetAllNotifications();
        }

        public async Task<List<Notification>> GetNotificationsByUserId(int userId)
        {
            return await _notificationRepository.GetNotificationsByUserId(userId);
        }

        public async Task UpdateNotification(NotificationRequest request)
        {
            await _notificationRepository.UpdateNotification(new Notification
            {
                Id = request.Id,
                Title = request.Title,
                Description = request.Description,
                DataType = request.DataType,
                DataId = request.DataId,
                UserId = request.UserId,
                Created = DateTime.UtcNow,
                StaffId = request.StaffId
            });
        }
    }
}
