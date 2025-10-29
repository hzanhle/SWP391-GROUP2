using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Models;
using UserService.Services;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IJwtService _jwtService;

        public NotificationController(INotificationService notificationService, IJwtService jwtService)
        {
            _notificationService = notificationService;
            _jwtService = jwtService;
        }

        private int GetUserIdFromToken()
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (string.IsNullOrEmpty(token))
                throw new UnauthorizedAccessException("Token không tồn tại.");

            var userId = _jwtService.GetUserIdFromToken(token);
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Không thể trích xuất UserId từ token.");

            return int.Parse(userId);
        }
        // Lấy tất cả notifications của user
        [Authorize(Roles = "Member")]
        [HttpGet]
        public async Task<IActionResult> GetNotificationsByUserId()
        {
            int userId = GetUserIdFromToken();
            var notifications = await _notificationService.GetAllByUserId(userId);
            return Ok(notifications); // trả về danh sách notifications
        }

        // Xóa tất cả notifications của user
        [Authorize(Roles = "Member")]
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteAllNotificationsByUserId()
        {
            int userId = GetUserIdFromToken();
            await _notificationService.RemoveNotificationByUserId(userId);
            return NoContent(); // 204 No Content
        }

        [HttpPut]
        [Authorize(Roles = "Admin,Employee")]
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
