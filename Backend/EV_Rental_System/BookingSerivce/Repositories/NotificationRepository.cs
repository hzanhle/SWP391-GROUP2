using BookingSerivce.Models;
using BookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingSerivce.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly MyDbContext _context;

        public NotificationRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task AddNotification(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteNotification(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();

            if (notifications.Any())
            {
                _context.Notifications.RemoveRange(notifications);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateNotification(Notification notification)
        {
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }
        public async Task<List<Notification>> GetAllNotifications()
        {
            return await _context.Notifications.ToListAsync();
        }

        public async Task<List<Notification>> GetNotificationsByUserId(int userId)
        {
            return await _context.Notifications
                                 .Where(n => n.UserId == userId)
                                 .ToListAsync();
        }        
    }
}
