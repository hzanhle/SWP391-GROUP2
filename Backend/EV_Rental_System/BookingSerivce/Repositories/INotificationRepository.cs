using BookingService.Models;

namespace BookingService.Repositories
{
    public interface INotificationRepository
    {
        Task<int> CreateAsync(Notification notification);
        Task<Notification?> GetByIdAsync(int id);
        Task<IEnumerable<Notification>> GetByUserIdAsync(int userId);
        Task<bool> UpdateAsync(Notification notification);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteByUserIdAsync(int userId);
        Task<IEnumerable<Notification>> GetByDataTypeAsync(string dataType);
        Task<IEnumerable<Notification>> GetByStaffIdAsync(int staffId);
    }
}
