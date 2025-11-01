// Repositories/FeedbackRepository.cs

using Microsoft.EntityFrameworkCore;
using StationService.Models;

namespace StationService.Repositories
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly MyDbContext _context;
        private readonly ILogger<FeedbackRepository> _logger;

        public FeedbackRepository(MyDbContext context, ILogger<FeedbackRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ==================== CRUD Operations ====================
        /// Lấy feedback theo ID
        public async Task<Feedback?> GetByIdAsync(int feedbackId)
        {
            try
            {
                return await _context.Feedbacks
                    .Include(f => f.Station)
                    .FirstOrDefaultAsync(f => f.FeedbackId == feedbackId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting feedback {feedbackId}");
                throw;
            }
        }

        /// Lấy tất cả feedback
        public async Task<List<Feedback>> GetAllAsync()
        {
            try
            {
                return await _context.Feedbacks
                    .Include(f => f.Station)
                    .OrderByDescending(f => f.CreatedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all feedbacks");
                throw;
            }
        }

        /// Tạo feedback mới
        public async Task<Feedback> CreateAsync(Feedback feedback)
        {
            try
            {
                _context.Feedbacks.Add(feedback);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Created feedback {feedback.FeedbackId}");

                // Reload with navigation properties
                return await GetByIdAsync(feedback.FeedbackId) ?? feedback;
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UQ_Feedback_UserStation") == true)
            {
                _logger.LogWarning($"User {feedback.UserId} already has feedback for station {feedback.StationId}");
                throw new InvalidOperationException("Bạn đã đánh giá trạm này rồi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating feedback");
                throw;
            }
        }

        /// Cập nhật feedback
        public async Task<Feedback> UpdateAsync(Feedback feedback)
        {
            try
            {
                _context.Feedbacks.Update(feedback);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Updated feedback {feedback.FeedbackId}");

                // Reload with navigation properties
                return await GetByIdAsync(feedback.FeedbackId) ?? feedback;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating feedback {feedback.FeedbackId}");
                throw;
            }
        }

        /// Xóa feedback
        public async Task<bool> DeleteAsync(int feedbackId)
        {
            try
            {
                var feedback = await _context.Feedbacks.FindAsync(feedbackId);
                if (feedback == null)
                {
                    return false;
                }

                _context.Feedbacks.Remove(feedback);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Deleted feedback {feedbackId}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting feedback {feedbackId}");
                throw;
            }
        }

        // ==================== Query Methods ====================
        /// Lấy tất cả feedback của một station
        public async Task<List<Feedback>> GetByStationIdAsync(int stationId, bool onlyPublished = true)
        {
            try
            {
                var query = _context.Feedbacks
                    .Include(f => f.Station)
                    .Where(f => f.StationId == stationId);

                if (onlyPublished)
                {
                    query = query.Where(f => f.IsPublished);
                }

                return await query
                    .OrderByDescending(f => f.CreatedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting feedbacks for station {stationId}");
                throw;
            }
        }

        /// Lấy tất cả feedback của một user
        public async Task<List<Feedback>> GetByUserIdAsync(int userId)
        {
            try
            {
                return await _context.Feedbacks
                    .Include(f => f.Station)
                    .Where(f => f.UserId == userId)
                    .OrderByDescending(f => f.CreatedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting feedbacks for user {userId}");
                throw;
            }
        }

        /// Lấy feedback của user cho một station cụ thể
        public async Task<Feedback?> GetByUserAndStationAsync(int userId, int stationId)
        {
            try
            {
                return await _context.Feedbacks
                    .Include(f => f.Station)
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.StationId == stationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting feedback for user {userId} and station {stationId}");
                throw;
            }
        }

        // ==================== Statistics ====================
        /// Đếm tổng số feedback của station
        public async Task<int> GetTotalFeedbacksForStationAsync(int stationId)
        {
            try
            {
                return await _context.Feedbacks
                    .Where(f => f.StationId == stationId && f.IsPublished)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error counting feedbacks for station {stationId}");
                throw;
            }
        }

        /// Tính rating trung bình của station
        public async Task<double> GetAverageRatingForStationAsync(int stationId)
        {
            try
            {
                var feedbacks = await _context.Feedbacks
                    .Where(f => f.StationId == stationId && f.IsPublished)
                    .ToListAsync();

                if (!feedbacks.Any())
                {
                    return 0;
                }

                return feedbacks.Average(f => f.Rate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calculating average rating for station {stationId}");
                throw;
            }
        }
    }
}