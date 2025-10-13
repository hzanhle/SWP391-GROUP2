using Microsoft.AspNetCore.Mvc;
using UserService.Models;
using UserService.Services;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // Lấy tất cả notifications của user
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetNotificationsByUserId(int userId)
        {
            var notifications = await _notificationService.GetAllByUserId(userId);
            return Ok(notifications); // trả về danh sách notifications
        }

        // Xóa tất cả notifications của user
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteAllNotificationsByUserId(int userId)
        {
            await _notificationService.RemoveNotificationByUserId(userId);
            return NoContent(); // 204 No Content
        }

        [HttpPut]
        public async Task<IActionResult> UpdateNotification([FromBody] Notification notification)
        {
            if (notification == null || notification.Id == 0)
            {
                return BadRequest("Invalid notification data.");
            }
            await _notificationService.UpdateNotification(notification);
            return NoContent(); // 204 No Content
        }
    }
}
