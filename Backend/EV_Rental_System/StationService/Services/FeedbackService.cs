using Microsoft.Extensions.Logging;
using StationService.DTOs;
using StationService.Models;
using StationService.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StationService.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly ILogger<FeedbackService> _logger;

        public FeedbackService(IFeedbackRepository feedbackRepository, ILogger<FeedbackService> logger)
        {
            _feedbackRepository = feedbackRepository;
            _logger = logger;
        }

        public async Task<FeedbackDTO> CreateAsync(int stationId, CreateFeedbackRequest request)
        {
            try
            {
                _logger.LogInformation("Đang tạo feedback mới cho OrderId: {OrderId}", request.OrderId);

                // Ánh xạ (map) từ DTO sang Model
                var feedback = new Feedback
                {
                    StationId = stationId,
                    OrderId = request.OrderId,
                    Rate = request.Rate,
                    Description = request.Description
                };

                var createdFeedback = await _feedbackRepository.AddAsync(feedback);
                _logger.LogInformation("Đã tạo thành công FeedbackId: {FeedbackId}", createdFeedback.FeedbackId);

                // Ánh xạ ngược lại từ Model sang DTO để trả về
                return MapToDto(createdFeedback);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo feedback cho OrderId: {OrderId}", request.OrderId);
                throw; // Ném lại lỗi để Controller xử lý
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Đang xóa FeedbackId: {FeedbackId}", id);

                // Kiểm tra xem feedback có tồn tại không trước khi xóa
                var existingFeedback = await _feedbackRepository.GetByIdAsync(id);
                if (existingFeedback == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy feedback với ID: {id}");
                }

                await _feedbackRepository.DeleteAsync(id);
                _logger.LogInformation("Đã xóa thành công FeedbackId: {FeedbackId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa FeedbackId: {FeedbackId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<FeedbackDTO>> GetByStationIdAsync(int stationId)
        {
            try
            {
                _logger.LogInformation("Đang lấy danh sách feedback cho StationId: {StationId}", stationId);
                var feedbacks = await _feedbackRepository.GetByStationIdAsync(stationId);

                // Dùng LINQ để chuyển đổi danh sách Model sang danh sách DTO
                return feedbacks.Select(f => MapToDto(f));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách feedback cho StationId: {StationId}", stationId);
                throw;
            }
        }

        public async Task<FeedbackDTO?> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Đang tìm FeedbackId: {FeedbackId}", id);
                var feedback = await _feedbackRepository.GetByIdAsync(id);

                if (feedback == null)
                {
                    return null; // Trả về null nếu không tìm thấy
                }

                return MapToDto(feedback);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm FeedbackId: {FeedbackId}", id);
                throw;
            }
        }

        public async Task UpdateAsync(int id, UpdateFeedbackRequest request)
        {
            try
            {
                _logger.LogInformation("Đang cập nhật FeedbackId: {FeedbackId}", id);
                var existingFeedback = await _feedbackRepository.GetByIdAsync(id);

                if (existingFeedback == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy feedback với ID: {id} để cập nhật.");
                }

                // Cập nhật các thuộc tính
                existingFeedback.Rate = request.Rate;
                existingFeedback.Description = request.Description;

                await _feedbackRepository.UpdateAsync(existingFeedback);
                _logger.LogInformation("Đã cập nhật thành công FeedbackId: {FeedbackId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật FeedbackId: {FeedbackId}", id);
                throw;
            }
        }

        private FeedbackDTO MapToDto(Feedback feedback)
        {
            return new FeedbackDTO
            {
                FeedbackId = feedback.FeedbackId,
                StationId = feedback.StationId,
                OrderId = feedback.OrderId,
                Rate = feedback.Rate,
                Description = feedback.Description,
                CreatedDate = feedback.CreatedDate
            };
        }
    }
}
