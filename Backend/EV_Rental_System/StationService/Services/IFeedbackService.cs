using StationService.DTOs;

namespace StationService.Services
{
    public interface IFeedbackService
    {
        // ==================== CRUD Operations ====================

        Task<FeedbackDTO?> GetByIdAsync(int feedbackId);
        Task<FeedbackDTO> CreateFeedbackAsync(CreateFeedbackDTO dto, int userId);
        Task<FeedbackDTO> UpdateFeedbackAsync(int feedbackId, UpdateFeedbackDTO dto, int userId);
        Task<bool> DeleteFeedbackAsync(int feedbackId, int userId, bool isAdmin = false);

        // ==================== Query Operations ====================
        /// Lấy tất cả feedback của một station (public)
        Task<List<FeedbackDTO>> GetFeedbacksByStationAsync(int stationId, bool onlyPublished = true);
        /// Lấy feedback của user cho một station cụ thể
        Task<FeedbackDTO?> GetMyFeedbackForStationAsync(int userId, int stationId);
        /// Lấy tất cả feedback mà user đã tạo
        Task<List<FeedbackDTO>> GetFeedbacksByUserAsync(int userId);

        /// Lấy tất cả feedback (Admin)
        Task<List<FeedbackDTO>> GetAllFeedbacksAsync();

        // ==================== Statistics ====================
        /// Lấy thống kê feedback của station
        Task<StationFeedbackStatsDTO> GetStationStatsAsync(int stationId);

        // ==================== Admin Operations ====================
        /// [Admin] Verify/Unverify feedback (xác minh phản hồi)
        Task<FeedbackDTO> VerifyFeedbackAsync(int feedbackId, bool isVerified);

        /// [Admin] Publish/Unpublish feedback (ẩn feedback spam)
        Task<FeedbackDTO> PublishFeedbackAsync(int feedbackId, bool isPublished);
    }
}