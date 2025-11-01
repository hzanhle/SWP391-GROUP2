// Controllers/FeedbackController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StationService.DTOs;
using StationService.Services;
using System.Security.Claims;

namespace StationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;
        private readonly ILogger<FeedbackController> _logger;

        public FeedbackController(
            IFeedbackService feedbackService,
            ILogger<FeedbackController> logger)
        {
            _feedbackService = feedbackService;
            _logger = logger;
        }

        // ==================== HELPER METHODS ====================

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid or missing user ID in token");
            }
            return userId;
        }

        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        // ==================== PUBLIC APIs (No Auth) ====================
        /// [Public] Xem tất cả feedback của một station
        [HttpGet("station/{stationId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStationFeedbacks(int stationId)
        {
            try
            {
                var feedbacks = await _feedbackService.GetFeedbacksByStationAsync(
                    stationId, onlyPublished: true);

                return Ok(new
                {
                    success = true,
                    data = feedbacks,
                    total = feedbacks.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting feedbacks for station {stationId}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra"
                });
            }
        }

        /// [Public] Xem thống kê rating của station
        [HttpGet("station/{stationId}/stats")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStationStats(int stationId)
        {
            try
            {
                var stats = await _feedbackService.GetStationStatsAsync(stationId);

                return Ok(new
                {
                    success = true,
                    data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting stats for station {stationId}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra"
                });
            }
        }

        /// [Public] Xem chi tiết một feedback
        [HttpGet("{feedbackId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFeedbackById(int feedbackId)
        {
            try
            {
                var feedback = await _feedbackService.GetByIdAsync(feedbackId);

                if (feedback == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy feedback"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = feedback
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting feedback {feedbackId}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra"
                });
            }
        }

        // ==================== USER APIs (Auth Required) ====================
        /// [User] Tạo feedback mới cho station
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateFeedback([FromBody] CreateFeedbackDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Dữ liệu không hợp lệ",
                        errors = ModelState
                    });
                }

                var userId = GetCurrentUserId();
                var feedback = await _feedbackService.CreateFeedbackAsync(dto, userId);

                _logger.LogInformation($"User {userId} created feedback {feedback.FeedbackId}");

                return Ok(new
                {
                    success = true,
                    message = "Tạo feedback thành công",
                    data = feedback
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating feedback");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi tạo feedback"
                });
            }
        }

        /// [User] Cập nhật feedback của mình
        [HttpPut("{feedbackId}")]
        [Authorize]
        public async Task<IActionResult> UpdateFeedback(
            int feedbackId,
            [FromBody] UpdateFeedbackDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Dữ liệu không hợp lệ",
                        errors = ModelState
                    });
                }

                var userId = GetCurrentUserId();
                var feedback = await _feedbackService.UpdateFeedbackAsync(feedbackId, dto, userId);

                _logger.LogInformation($"User {userId} updated feedback {feedbackId}");

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật feedback thành công",
                    data = feedback
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating feedback {feedbackId}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi cập nhật feedback"
                });
            }
        }

        /// [User] Xóa feedback của mình
        [HttpDelete("{feedbackId}")]
        [Authorize]
        public async Task<IActionResult> DeleteFeedback(int feedbackId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var isAdmin = IsAdmin();

                await _feedbackService.DeleteFeedbackAsync(feedbackId, userId, isAdmin);

                _logger.LogInformation($"User {userId} deleted feedback {feedbackId}");

                return Ok(new
                {
                    success = true,
                    message = "Xóa feedback thành công"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting feedback {feedbackId}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi xóa feedback"
                });
            }
        }

        /// [User] Xem tất cả feedback đã tạo
        [HttpGet("my-feedbacks")]
        [Authorize]
        public async Task<IActionResult> GetMyFeedbacks()
        {
            try
            {
                var userId = GetCurrentUserId();
                var feedbacks = await _feedbackService.GetFeedbacksByUserAsync(userId);

                return Ok(new
                {
                    success = true,
                    data = feedbacks,
                    total = feedbacks.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user feedbacks");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra"
                });
            }
        }

        /// [User] Xem feedback của mình cho một station
        [HttpGet("my-feedback/station/{stationId}")]
        [Authorize]
        public async Task<IActionResult> GetMyFeedbackForStation(int stationId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var feedback = await _feedbackService.GetMyFeedbackForStationAsync(userId, stationId);

                if (feedback == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Bạn chưa feedback cho trạm này"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = feedback
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user feedback for station {stationId}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra"
                });
            }
        }

        // ==================== ADMIN APIs ====================
        /// [Admin] Xem tất cả feedback
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllFeedbacks()
        {
            try
            {
                var feedbacks = await _feedbackService.GetAllFeedbacksAsync();

                return Ok(new
                {
                    success = true,
                    data = feedbacks,
                    total = feedbacks.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all feedbacks");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra"
                });
            }
        }

        /// [Admin] Verify/Unverify feedback
        [HttpPut("{feedbackId}/verify")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> VerifyFeedback(
            int feedbackId,
            [FromBody] VerifyFeedbackRequest request)
        {
            try
            {
                var feedback = await _feedbackService.VerifyFeedbackAsync(
                    feedbackId, request.IsVerified);

                _logger.LogInformation($"Admin verified feedback {feedbackId}: {request.IsVerified}");

                return Ok(new
                {
                    success = true,
                    message = request.IsVerified ? "Đã verify feedback" : "Đã bỏ verify feedback",
                    data = feedback
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verifying feedback {feedbackId}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra"
                });
            }
        }

        /// [Admin] Publish/Unpublish feedback (ẩn spam)
        [HttpPut("{feedbackId}/publish")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PublishFeedback(
            int feedbackId,
            [FromBody] PublishFeedbackRequest request)
        {
            try
            {
                var feedback = await _feedbackService.PublishFeedbackAsync(
                    feedbackId, request.IsPublished);

                _logger.LogInformation($"Admin set publish feedback {feedbackId}: {request.IsPublished}");

                return Ok(new
                {
                    success = true,
                    message = request.IsPublished ? "Đã hiển thị feedback" : "Đã ẩn feedback",
                    data = feedback
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error publishing feedback {feedbackId}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra"
                });
            }
        }
    }

    // ==================== REQUEST DTOs ====================

    public class VerifyFeedbackRequest
    {
        public bool IsVerified { get; set; }
    }

    public class PublishFeedbackRequest
    {
        public bool IsPublished { get; set; }
    }
}