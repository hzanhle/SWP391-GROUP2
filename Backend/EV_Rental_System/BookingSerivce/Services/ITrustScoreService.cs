using BookingService.Models;
namespace BookingService.Services
{
    public interface ITrustScoreService
    {
        /**
         * Lấy điểm số hiện tại của người dùng.
         * Trả về 0 nếu người dùng chưa có điểm.
         */
        Task<int> GetCurrentScoreAsync(int userId);

        /**
         * Cập nhật điểm khi thanh toán lần đầu thành công.
         * Service sẽ tự kiểm tra và tạo mới nếu user chưa có điểm.
         */
        Task UpdateScoreOnFirstPaymentAsync(int userId, int orderId);

        /**
         * Cập nhật điểm khi hoàn thành một chuyến đi.
         */
        Task UpdateScoreOnRentalCompletionAsync(int userId, int orderId);

        /**
         * Cập nhật (trừ) điểm khi người dùng không đến nhận xe (NoShow).
         */
        Task UpdateScoreOnNoShowAsync(int userId, int orderId);

        /**
         * Lấy toàn bộ object TrustScore (ví dụ: cho trang admin).
         */
        Task<TrustScore?> GetFullTrustScoreAsync(int userId);

        /**
         * Lấy danh sách người dùng có điểm cao nhất.
         */
        Task<List<TrustScore>> GetTopScoresAsync();

        /**
         * Lấy điểm trung bình của toàn hệ thống.
         */
        Task<double> GetAverageScoreAsync();
    }
}
