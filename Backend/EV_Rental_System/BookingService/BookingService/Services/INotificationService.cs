using BookingService.Models;
namespace BookingService.Services
{
   

    public interface INotificationService
    {
        /**
         * Tạo một thông báo mới cho người dùng.
         * Đây là nơi logic nghiệp vụ (như set DateTime.UtcNow) được thực thi.
         */
        Task<Notification> CreateNotificationAsync(
            int userId,
            string title,
            string description,
            string dataType,
            int? dataId,
            int? staffId = null
        );

        /**
         * Lấy thông báo bằng ID.
         */
        Task<Notification?> GetNotificationByIdAsync(int id);

        /**
         * Lấy tất cả thông báo cho một người dùng cụ thể.
         */
        Task<IEnumerable<Notification>> GetNotificationsForUserAsync(int userId);

        /**
         * Xóa một thông báo bằng ID.
         */
        Task<bool> DeleteNotificationAsync(int id);

        /**
         * Xóa tất cả thông báo của một người dùng.
         */
        Task<bool> DeleteAllNotificationsForUserAsync(int userId);

        /**
         * Lấy thông báo theo loại dữ liệu (ví dụ: "OrderCreated").
         */
        Task<IEnumerable<Notification>> GetNotificationsByTypeAsync(string dataType);

        /**
         * Lấy thông báo được tạo bởi một nhân viên.
         */
        Task<IEnumerable<Notification>> GetNotificationsByStaffAsync(int staffId);
    }
}
