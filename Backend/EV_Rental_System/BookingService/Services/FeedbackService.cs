using BookingService.Models;
using BookingService.Repositories;

namespace BookingService.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedBackRepository _feedbackRepository;

        public FeedbackService(IFeedBackRepository feedbackRepository)
        {
            _feedbackRepository = feedbackRepository;
        }

        public async Task<Feedback?> GetByFeedbackIdAsync(int id)
        {
            try
            {
                return await _feedbackRepository.GetByFeedBackIdAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy feedback ID = {id}: {ex.Message}", ex);
            }
        }

        public async Task<Feedback?> GetByOrderIdAsync(int orderId)
        {
            try
            {
                return await _feedbackRepository.GetByOrderIdAsync(orderId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy feedback theo OrderId = {orderId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Feedback>> GetByUserIdAsync(int userId)
        {
            try
            {
                return await _feedbackRepository.GetByUserIdAsync(userId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy danh sách feedback của UserId = {userId}: {ex.Message}", ex);
            }
        }

        public async Task CreateAsync(Feedback feedback)
        {
            try
            {
                if (feedback == null)
                {
                    throw new ArgumentNullException(nameof(feedback));
                }

                var existing = await _feedbackRepository.GetByOrderIdAsync(feedback.OrderId);
                if (existing != null)
                {
                    existing.Rating = feedback.Rating;
                    existing.Comments = feedback.Comments;
                    existing.VehicleId = feedback.VehicleId;
                    existing.UserId = feedback.UserId;

                    await _feedbackRepository.UpdateAsync(existing);
                    feedback.FeedbackId = existing.FeedbackId;
                }
                else
                {
                    feedback.Created = DateTime.UtcNow;
                    await _feedbackRepository.CreateAsync(feedback);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi tạo mới feedback: " + ex.Message, ex);
            }
        }

        public async Task UpdateAsync(Feedback feedback)
        {
            try
            {
                var existing = await _feedbackRepository.GetByFeedBackIdAsync(feedback.FeedbackId);
                if (existing == null)
                    throw new KeyNotFoundException($"Feedback với ID = {feedback.FeedbackId} không tồn tại.");

                await _feedbackRepository.UpdateAsync(feedback);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi cập nhật feedback: " + ex.Message, ex);
            }
        }

        public async Task DeleteAsync(int feedbackId)
        {
            try
            {
                var existing = await _feedbackRepository.GetByFeedBackIdAsync(feedbackId);
                if (existing == null)
                    throw new KeyNotFoundException($"Feedback với ID = {feedbackId} không tồn tại.");

                await _feedbackRepository.DeleteAsync(existing);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi xóa feedback: " + ex.Message, ex);
            }
        }
    }
}
