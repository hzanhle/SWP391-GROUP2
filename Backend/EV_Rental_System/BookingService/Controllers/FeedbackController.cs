using BookingService.Models;
using BookingService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;
        private readonly ILogger<FeedbackController> _logger;

        public FeedbackController(IFeedbackService feedbackService, ILogger<FeedbackController> logger)
        {
            _feedbackService = feedbackService;
            _logger = logger;
        }

        // 🟢 [GET] api/feedback/{id}
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Employee,Member")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var feedback = await _feedbackService.GetByFeedbackIdAsync(id);
                if (feedback == null)
                    return NotFound(new { message = "Feedback không tồn tại." });

                return Ok(feedback);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy feedback ID = {Id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // 🟢 [GET] api/feedback/order/{orderId}
        [HttpGet("order/{orderId:int}")]
        [Authorize(Roles = "Admin,Employee,Member")]
        public async Task<IActionResult> GetByOrderId(int orderId)
        {
            try
            {
                var feedback = await _feedbackService.GetByOrderIdAsync(orderId);
                if (feedback == null)
                    return NotFound(new { message = "Không tìm thấy feedback cho đơn hàng này." });

                return Ok(feedback);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy feedback theo OrderId = {OrderId}", orderId);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // 🟢 [GET] api/feedback/user/{userId}
        [HttpGet("user/{userId:int}")]
        [Authorize(Roles = "Admin,Employee,Member")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            try
            {
                var feedbacks = await _feedbackService.GetByUserIdAsync(userId);
                return Ok(feedbacks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách feedback của UserId = {UserId}", userId);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // 🟢 [POST] api/feedback
        [HttpPost]
        [Authorize(Roles = "Member,Admin")]
        public async Task<IActionResult> Create([FromBody] Feedback feedback)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                await _feedbackService.CreateAsync(feedback);
                return CreatedAtAction(nameof(GetById), new { id = feedback.FeedbackId }, feedback);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo feedback mới");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // 🟡 [PUT] api/feedback/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Member,Employee,Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] Feedback feedback)
        {
            try
            {
                if (id != feedback.FeedbackId)
                    return BadRequest(new { message = "ID không khớp." });

                await _feedbackService.UpdateAsync(feedback);
                return Ok(new { message = "Cập nhật feedback thành công." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật feedback ID = {Id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // 🔴 [DELETE] api/feedback/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _feedbackService.DeleteAsync(id);
                return Ok(new { message = "Xóa feedback thành công." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa feedback ID = {Id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
