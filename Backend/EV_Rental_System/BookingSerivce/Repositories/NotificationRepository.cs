using BookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly MyDbContext _context;

        public NotificationRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<int> CreateAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification.Id ?? 0;
        }

        public async Task<Notification?> GetByIdAsync(int id)
        {
            return await _context.Notifications.FindAsync(id);
        }

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.Created)
                .ToListAsync();
        }

        public async Task<bool> UpdateAsync(Notification notification)
        {
            var existingNotification = await _context.Notifications.FindAsync(notification.Id);
            if (existingNotification == null) return false;

            existingNotification.Title = notification.Title;
            existingNotification.Description = notification.Description;
            existingNotification.DataType = notification.DataType;
            existingNotification.DataId = notification.DataId;
            existingNotification.StaffId = notification.StaffId;
            existingNotification.UserId = notification.UserId;
            existingNotification.Created = notification.Created;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return false;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteByUserIdAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();

            if (!notifications.Any()) return false;

            _context.Notifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Notification>> GetByDataTypeAsync(string dataType)
        {
            return await _context.Notifications
                .Where(n => n.DataType == dataType)
                .OrderByDescending(n => n.Created)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetByStaffIdAsync(int staffId)
        {
            return await _context.Notifications
                .Where(n => n.StaffId == staffId)
                .OrderByDescending(n => n.Created)
                .ToListAsync();
        }
    }
}
