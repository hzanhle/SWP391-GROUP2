using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StationService.DTOs;
using StationService.Services;
using System;
using System.Threading.Tasks;

namespace StationService.Controllers
{
    // File: Controllers/FeedbackController.cs
    [ApiController]
    [Route("api/stations/{stationId}/feedbacks")]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;
        private readonly ILogger<FeedbackController> _logger;

        public FeedbackController(IFeedbackService feedbackService, ILogger<FeedbackController> logger)
        {
            _feedbackService = feedbackService;
            _logger = logger;
        }

        // GET /api/stations/1/feedbacks
        [HttpGet]
        [AllowAnonymous] // Cho phép khách vãng lai xem feedback
        public async Task<IActionResult> GetFeedbacksForStation(int stationId)
        {
            try
            {
                var feedbacks = await _feedbackService.GetByStationIdAsync(stationId);
                return Ok(feedbacks); // Trả về 200 OK và danh sách feedback
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã xảy ra lỗi khi lấy danh sách feedback cho StationId: {StationId}", stationId);
                return StatusCode(500, "Đã xảy ra lỗi khi xử lý yêu cầu của bạn.");
            }
        }

        // POST /api/stations/1/feedbacks
        [HttpPost]
        [Authorize(Roles = "User,Admin")] // Chỉ người dùng đã đăng nhập mới có thể tạo feedback
        public async Task<IActionResult> CreateFeedback(int stationId, [FromBody] CreateFeedbackRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Trả về 400 Bad Request nếu dữ liệu không hợp lệ
            }

            try
            {
                var newFeedback = await _feedbackService.CreateAsync(stationId, request);
                //Trả về 201 Created và thông tin feedback vừa tạo
                return CreatedAtRoute("GetFeedbackByIdRoute", new { stationId = stationId, feedbackId = newFeedback.FeedbackId }, newFeedback);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo Feedback cho stationId: {StationId}", stationId);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống.");
            }
        }

        // GET: /api/stations/1/feedbacks/10
        [HttpGet("{feedbackId}", Name = "GetFeedbackByIdRoute")]
        [AllowAnonymous] //Cho phép khách vãng lai tìm xem feedback theo id trạm
        public async Task<IActionResult> GetFeedbackById(int stationId, int feedbackId)
        {
            try
            {
                var feedback = await _feedbackService.GetByIdAsync(feedbackId);
                if (feedback == null || feedback.StationId != stationId)
                {
                    return NotFound();
                }
                return Ok(feedback);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy feedback ID: {FeedbackId}", feedbackId);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống.");
            }
        }

        // PUT: /api/stations/1/feedbacks/10
        [HttpPut("{feedbackId}")]
        [Authorize(Roles = "Admin")] // Chỉ Admin mới có quyền sửa feedback
        public async Task<IActionResult> UpdateFeedback(int stationId, int feedbackId, [FromBody] UpdateFeedbackRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _feedbackService.UpdateAsync(feedbackId, request);
                return NoContent(); // 204 No Content - Thành công
            }
            catch (KeyNotFoundException knfex)
            {
                _logger.LogWarning(knfex.Message);
                return NotFound(knfex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật feedback ID: {FeedbackId}", feedbackId);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống.");
            }
        }

        // DELETE: /api/stations/1/feedbacks/10
        [HttpDelete("{feedbackId}")]
        [Authorize(Roles = "Admin")] // Chỉ Admin mới có quyền xóa feedback
        public async Task<IActionResult> DeleteFeedback(int stationId, int feedbackId)
        {
            try
            {
                var feedbackToDelete = await _feedbackService.GetByIdAsync(feedbackId);
                if (feedbackToDelete == null)
                {
                    _logger.LogWarning("Không tìm thấy Feedback Id: {feedbackId} để xóa.", feedbackId);
                    return NotFound($"Không tìm thấy feedback với ID: {feedbackId}.");
                }

                //Kiểm tra feedback có thuộc đúng station không
                if(feedbackToDelete.StationId != stationId)
                {
                    _logger.LogWarning("Cố gắng xóa FeedbackId: {FeedbackId} (thuộc StationId: {ActualStationId}) thông qua URL của StationId: {RequestStationId}",
                                        feedbackId, feedbackToDelete.StationId, stationId);
                    return Forbid();
                }

                await _feedbackService.DeleteAsync(feedbackId);
                _logger.LogInformation("Đã xóa thành công FeedbackId: {FeedbackId} thuộc StationId: {StationId}.", feedbackId, stationId);
                return NoContent();
            }
            catch (KeyNotFoundException knfex)
            {
                _logger.LogWarning(knfex.Message);
                return NotFound(knfex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa feedback ID: {FeedbackId} cho StationId: {StaionId}", feedbackId, stationId);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống.");
            }
        }
    }
}
