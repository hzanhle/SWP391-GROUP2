using BookingService.Models;
using BookingService.Repositories;
using BookingService.Services;


public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepo;
    private readonly ILogger<NotificationService> _logger;

    // Inject Repository và Logger qua constructor
    public NotificationService(
        INotificationRepository notificationRepo,
        ILogger<NotificationService> logger)
    {
        _notificationRepo = notificationRepo;
        _logger = logger; 
    }

    public async Task<Notification> CreateNotificationAsync(
        int userId,
        string title,
        string description,
        string dataType,
        int? dataId,
        int? staffId = null)
    {
        // Logic nghiệp vụ: Service chịu trách nhiệm tạo đối tượng
        // và gán các giá trị mặc định như thời gian tạo.
        var notification = new Notification(
            title,
            description,
            dataType,
            dataId,
            staffId,
            userId,
            DateTime.UtcNow // Service quyết định thời gian tạo
        );

        try
        {
            // Gọi repository để lưu
            int newId = await _notificationRepo.CreateAsync(notification);

            // Gán ID vừa được tạo vào đối tượng
            notification.Id = newId;

            _logger.LogInformation("Notification {NotificationId} created for User {UserId}", newId, userId);
            return notification;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating notification for User {UserId}", userId);
            throw; // Ném lỗi ra để tầng trên (Controller/Job) xử lý
        }
    }

    public async Task<Notification?> GetNotificationByIdAsync(int id)
    {
        return await _notificationRepo.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Notification>> GetNotificationsForUserAsync(int userId)
    {
        return await _notificationRepo.GetByUserIdAsync(userId);
    }

    public async Task<bool> DeleteNotificationAsync(int id)
    {
        try
        {
            return await _notificationRepo.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification {NotificationId}", id);
            return false;
        }
    }

    public async Task<bool> DeleteAllNotificationsForUserAsync(int userId)
    {
        try
        {
            return await _notificationRepo.DeleteByUserIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting all notifications for User {UserId}", userId);
            return false;
        }
    }

    public async Task<IEnumerable<Notification>> GetNotificationsByTypeAsync(string dataType)
    {
        return await _notificationRepo.GetByDataTypeAsync(dataType);
    }

    public async Task<IEnumerable<Notification>> GetNotificationsByStaffAsync(int staffId)
    {
        return await _notificationRepo.GetByStaffIdAsync(staffId);
    }
}