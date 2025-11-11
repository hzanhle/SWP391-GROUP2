using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookingService.DTOs;
using BookingService.Services;

namespace BookingService.Controllers
{
    [ApiController]
    [Route("api/trustscore")]
    [Authorize]
    public class TrustScoreController : ControllerBase
    {
        private readonly ITrustScoreService _trustScoreService;
        private readonly ILogger<TrustScoreController> _logger;

        public TrustScoreController(
            ITrustScoreService trustScoreService,
            ILogger<TrustScoreController> logger)
        {
            _trustScoreService = trustScoreService;
            _logger = logger;
        }

        /// <summary>
        /// User views their own trust score
        /// </summary>
        [HttpGet("me")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> GetMyTrustScore()
        {
            try
            {
                // Get userId from JWT token
                var userIdClaim = User.FindFirst("userId") ?? User.FindFirst("sub");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { Message = "Invalid user token" });
                }

                var trustScore = await _trustScoreService.GetFullTrustScoreAsync(userId);
                if (trustScore == null)
                {
                    return Ok(new TrustScoreResponse
                    {
                        UserId = userId,
                        Score = 0,
                        LastRelatedOrderId = 0,
                        LastUpdated = DateTime.UtcNow,
                        RecentHistory = new List<TrustScoreHistoryResponse>()
                    });
                }

                // Get recent history
                var history = await _trustScoreService.GetRecentUserScoreHistoryAsync(userId, 5);
                var historyDto = history.Select(h => new TrustScoreHistoryResponse
                {
                    HistoryId = h.HistoryId,
                    UserId = h.UserId,
                    OrderId = h.OrderId,
                    ChangeAmount = h.ChangeAmount,
                    PreviousScore = h.PreviousScore,
                    NewScore = h.NewScore,
                    Reason = h.Reason,
                    ChangeType = h.ChangeType,
                    AdjustedByAdminId = h.AdjustedByAdminId,
                    CreatedAt = h.CreatedAt
                }).ToList();

                return Ok(new TrustScoreResponse
                {
                    UserId = trustScore.UserId,
                    Score = trustScore.Score,
                    LastRelatedOrderId = trustScore.OrderId,
                    LastUpdated = trustScore.CreatedAt,
                    RecentHistory = historyDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trust score for current user");
                return StatusCode(500, new { Message = "Error retrieving trust score" });
            }
        }

        /// <summary>
        /// Admin/Employee views any user's trust score
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetUserTrustScore(int userId)
        {
            try
            {
                var trustScore = await _trustScoreService.GetFullTrustScoreAsync(userId);
                if (trustScore == null)
                {
                    return Ok(new TrustScoreResponse
                    {
                        UserId = userId,
                        Score = 0,
                        LastRelatedOrderId = 0,
                        LastUpdated = DateTime.UtcNow,
                        RecentHistory = new List<TrustScoreHistoryResponse>()
                    });
                }

                // Get recent history
                var history = await _trustScoreService.GetRecentUserScoreHistoryAsync(userId, 10);
                var historyDto = history.Select(h => new TrustScoreHistoryResponse
                {
                    HistoryId = h.HistoryId,
                    UserId = h.UserId,
                    OrderId = h.OrderId,
                    ChangeAmount = h.ChangeAmount,
                    PreviousScore = h.PreviousScore,
                    NewScore = h.NewScore,
                    Reason = h.Reason,
                    ChangeType = h.ChangeType,
                    AdjustedByAdminId = h.AdjustedByAdminId,
                    CreatedAt = h.CreatedAt
                }).ToList();

                return Ok(new TrustScoreResponse
                {
                    UserId = trustScore.UserId,
                    Score = trustScore.Score,
                    LastRelatedOrderId = trustScore.OrderId,
                    LastUpdated = trustScore.CreatedAt,
                    RecentHistory = historyDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trust score for User {UserId}", userId);
                return StatusCode(500, new { Message = "Error retrieving trust score" });
            }
        }

        /// <summary>
        /// Get full trust score history for a user (Admin/Employee only)
        /// </summary>
        [HttpGet("history/{userId}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetUserTrustScoreHistory(int userId)
        {
            try
            {
                var history = await _trustScoreService.GetUserScoreHistoryAsync(userId);
                var historyDto = history.Select(h => new TrustScoreHistoryResponse
                {
                    HistoryId = h.HistoryId,
                    UserId = h.UserId,
                    OrderId = h.OrderId,
                    ChangeAmount = h.ChangeAmount,
                    PreviousScore = h.PreviousScore,
                    NewScore = h.NewScore,
                    Reason = h.Reason,
                    ChangeType = h.ChangeType,
                    AdjustedByAdminId = h.AdjustedByAdminId,
                    CreatedAt = h.CreatedAt
                }).ToList();

                return Ok(historyDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trust score history for User {UserId}", userId);
                return StatusCode(500, new { Message = "Error retrieving trust score history" });
            }
        }

        /// <summary>
        /// Admin manually adjusts a user's trust score
        /// </summary>
        [HttpPost("adjust")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdjustTrustScore([FromBody] TrustScoreAdjustmentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Get adminId from JWT token
                var adminIdClaim = User.FindFirst("userId") ?? User.FindFirst("sub");
                if (adminIdClaim == null || !int.TryParse(adminIdClaim.Value, out int adminId))
                {
                    return Unauthorized(new { Message = "Invalid admin token" });
                }

                await _trustScoreService.ManuallyAdjustScoreAsync(
                    request.UserId,
                    request.ChangeAmount,
                    request.Reason,
                    adminId);

                var updatedScore = await _trustScoreService.GetFullTrustScoreAsync(request.UserId);

                return Ok(new
                {
                    Message = "Trust score adjusted successfully",
                    UserId = request.UserId,
                    ChangeAmount = request.ChangeAmount,
                    NewScore = updatedScore?.Score ?? 0,
                    Reason = request.Reason,
                    AdjustedBy = adminId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adjusting trust score for User {UserId}", request.UserId);
                return StatusCode(500, new { Message = "Error adjusting trust score" });
            }
        }
    }
}
