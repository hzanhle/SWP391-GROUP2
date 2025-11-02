// Services/FeedbackService.cs - Core methods

using StationService.DTOs;
using StationService.Models;
using StationService.Repositories;
using Microsoft.Extensions.Logging;

namespace StationService.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _repository;
        private readonly IUserIntegrationService _userIntegrationService;
        private readonly ILogger<FeedbackService> _logger;

        public FeedbackService(
            IFeedbackRepository repository,
            IUserIntegrationService userIntegrationService,
            ILogger<FeedbackService> logger)
        {
            _repository = repository;
            _userIntegrationService = userIntegrationService;
            _logger = logger;
        }

        /// Tạo feedback cho station     
        public async Task<FeedbackDTO> CreateFeedbackAsync(CreateFeedbackDTO dto, int userId, string? userName)
        {
            _logger.LogInformation($"User {userId} creating feedback for station {dto.StationId}");

            // 1. Kiểm tra user đã feedback cho station này chưa
            var existingFeedback = await _repository.GetByUserAndStationAsync(userId, dto.StationId);
            if (existingFeedback != null)
            {
                throw new InvalidOperationException(
                    "Bạn đã đánh giá trạm này rồi. Vui lòng cập nhật feedback hiện tại thay vì tạo mới.");
            }

            // 2. Tạo feedback mới
            var feedback = new Feedback
            {
                UserId = userId,
                UserName = userName ?? $"User #{userId}",
                StationId = dto.StationId,
                Rate = dto.Rate,
                Description = dto.Description,
                CreatedDate = DateTime.UtcNow,
                IsPublished = true,
                IsVerified = false
            };

            var created = await _repository.CreateAsync(feedback);

            _logger.LogInformation($"Feedback {created.FeedbackId} created successfully");

            return MapToDto(created);
        }

        /// Cập nhật feedback
        public async Task<FeedbackDTO> UpdateFeedbackAsync(int feedbackId, UpdateFeedbackDTO dto, int userId)
        {
            _logger.LogInformation($"User {userId} updating feedback {feedbackId}");

            var feedback = await _repository.GetByIdAsync(feedbackId);
            if (feedback == null)
            {
                throw new KeyNotFoundException($"Feedback {feedbackId} not found");
            }

            // Kiểm tra ownership
            if (feedback.UserId != userId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền sửa feedback này");
            }

            // Cập nhật
            if (dto.Rate.HasValue)
            {
                feedback.Rate = dto.Rate.Value;
            }

            if (dto.Description != null)
            {
                feedback.Description = dto.Description;
            }

            feedback.UpdatedDate = DateTime.UtcNow;

            var updated = await _repository.UpdateAsync(feedback);

            _logger.LogInformation($"Feedback {feedbackId} updated successfully");

            return MapToDto(updated);
        }

        /// Xóa feedback (chỉ owner hoặc admin)
        public async Task<bool> DeleteFeedbackAsync(int feedbackId, int userId, bool isAdmin = false)
        {
            _logger.LogInformation($"User {userId} deleting feedback {feedbackId}");

            var feedback = await _repository.GetByIdAsync(feedbackId);
            if (feedback == null)
            {
                throw new KeyNotFoundException($"Feedback {feedbackId} not found");
            }

            // Kiểm tra quyền: Owner hoặc Admin
            if (!isAdmin && feedback.UserId != userId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xóa feedback này");
            }

            var result = await _repository.DeleteAsync(feedbackId);

            _logger.LogInformation($"Feedback {feedbackId} deleted successfully");

            return result;
        }

        /// Lấy tất cả feedback của một station (public API)
        public async Task<List<FeedbackDTO>> GetFeedbacksByStationAsync(int stationId, bool onlyPublished = true)
        {
            _logger.LogInformation($"Getting feedbacks for station {stationId}");

            var feedbacks = await _repository.GetByStationIdAsync(stationId, onlyPublished);

            var dtos = feedbacks.Select(MapToDto).ToList();
            await EnrichWithUserNamesAsync(dtos);
            
            return dtos;
        }
        /// Lấy feedback của user cho một station
        public async Task<FeedbackDTO?> GetMyFeedbackForStationAsync(int userId, int stationId)
        {
            _logger.LogInformation($"Getting feedback by user {userId} for station {stationId}");

            var feedback = await _repository.GetByUserAndStationAsync(userId, stationId);

            return feedback == null ? null : MapToDto(feedback);
        }

        /// Lấy tất cả feedback của user
        public async Task<List<FeedbackDTO>> GetFeedbacksByUserAsync(int userId)
        {
            _logger.LogInformation($"Getting all feedbacks by user {userId}");

            var feedbacks = await _repository.GetByUserIdAsync(userId);

            var dtos = feedbacks.Select(MapToDto).ToList();
            await EnrichWithUserNamesAsync(dtos);

            return dtos;
        }

        /// Lấy tất cả feedback (Admin only)
        public async Task<List<FeedbackDTO>> GetAllFeedbacksAsync()
        {
            _logger.LogInformation("Getting all feedbacks (Admin)");

            var feedbacks = await _repository.GetAllAsync();

            var dtos = feedbacks.Select(MapToDto).ToList();
            await EnrichWithUserNamesAsync(dtos);

            return dtos;
        }

        /// Lấy thống kê feedback của station
        public async Task<StationFeedbackStatsDTO> GetStationStatsAsync(int stationId)
        {
            _logger.LogInformation($"Getting stats for station {stationId}");

            var feedbacks = await _repository.GetByStationIdAsync(stationId, onlyPublished: true);

            var stats = new StationFeedbackStatsDTO
            {
                StationId = stationId,
                TotalFeedbacks = feedbacks.Count,
                AverageRating = feedbacks.Any() ? feedbacks.Average(f => f.Rate) : 0,
                FiveStar = feedbacks.Count(f => f.Rate == 5),
                FourStar = feedbacks.Count(f => f.Rate == 4),
                ThreeStar = feedbacks.Count(f => f.Rate == 3),
                TwoStar = feedbacks.Count(f => f.Rate == 2),
                OneStar = feedbacks.Count(f => f.Rate == 1)
            };

            return stats;
        }

        /// [Admin] Verify feedback
        public async Task<FeedbackDTO> VerifyFeedbackAsync(int feedbackId, bool isVerified)
        {
            var feedback = await _repository.GetByIdAsync(feedbackId);
            if (feedback == null)
            {
                throw new KeyNotFoundException($"Feedback {feedbackId} not found");
            }

            feedback.IsVerified = isVerified;
            feedback.UpdatedDate = DateTime.UtcNow;

            var updated = await _repository.UpdateAsync(feedback);

            _logger.LogInformation($"Feedback {feedbackId} verification set to {isVerified}");

            return MapToDto(updated);
        }

        /// [Admin] Publish/Unpublish feedback (ẩn spam)
        public async Task<FeedbackDTO> PublishFeedbackAsync(int feedbackId, bool isPublished)
        {
            var feedback = await _repository.GetByIdAsync(feedbackId);
            if (feedback == null)
            {
                throw new KeyNotFoundException($"Feedback {feedbackId} not found");
            }

            feedback.IsPublished = isPublished;
            feedback.UpdatedDate = DateTime.UtcNow;

            var updated = await _repository.UpdateAsync(feedback);

            _logger.LogInformation($"Feedback {feedbackId} published set to {isPublished}");

            return MapToDto(updated);
        }

        /// Lấy feedback theo feedbackid
        public async Task<FeedbackDTO?> GetByIdAsync(int feedbackId)
        {
            _logger.LogInformation($"Getting feedback by ID: {feedbackId}");

            var feedback = await _repository.GetByIdAsync(feedbackId);
            if (feedback == null)
            {
                _logger.LogWarning($"Feedback {feedbackId} not found");
                return null;
            }

            return MapToDto(feedback);
        }


        // ==================== HELPER ====================

        private FeedbackDTO MapToDto(Feedback feedback)
        {
            return new FeedbackDTO
            {
                FeedbackId = feedback.FeedbackId,
                StationId = feedback.StationId,
                StationName = feedback.Station?.Name,
                UserId = feedback.UserId,
                UserName = feedback.UserName,
                Rate = feedback.Rate,
                Description = feedback.Description,
                CreatedDate = feedback.CreatedDate,
                UpdatedDate = feedback.UpdatedDate,
                IsVerified = feedback.IsVerified,
                IsPublished = feedback.IsPublished
            };
        }

        /// Enrich DTOs with userName from UserService
        private async Task EnrichWithUserNamesAsync(List<FeedbackDTO> feedbacks)
        {
            if (!feedbacks.Any())
            {
                return;
            }

            try
            {
                // Get unique user IDs
                var userIds = feedbacks.Select(f => f.UserId).Distinct().ToList();

                // Fetch usernames in batch
                var userNames = await _userIntegrationService.GetUserNamesByIdsAsync(userIds);

                // Enrich each feedback
                foreach (var feedback in feedbacks)
                {
                    if (userNames.TryGetValue(feedback.UserId, out var userName))
                    {
                        feedback.UserName = userName;
                    }
                    else
                    {
                        feedback.UserName = $"User #{feedback.UserId}"; // Fallback
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to enrich feedbacks with usernames");
                // Không throw exception, chỉ log warning
                // Feedback vẫn được trả về nhưng không có userName
            }
        }
    }
}