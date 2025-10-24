using BookingService.Models;
namespace BookingService.Services
{
    public interface IPaymentService
    {
        /**
         * Tạo một bản ghi thanh toán mới (ở trạng thái Pending) cho một Order.
         * Được gọi bởi OrderService.
         */
        Task<Payment> CreatePaymentForOrderAsync(int orderId, decimal amount, string paymentMethod);

        /**
         * Đánh dấu thanh toán là 'Completed' (Hoàn thành).
         * Được gọi bởi Webhook/Callback từ cổng thanh toán.
         */
        Task<bool> MarkPaymentCompletedAsync(int orderId, string transactionId, string? gatewayResponse);

        /**
         * Đánh dấu thanh toán là 'Failed' (Thất bại).
         * Được gọi bởi Webhook/Callback hoặc nếu có lỗi.
         */
        Task<bool> MarkPaymentFailedAsync(int orderId, string? gatewayResponse);

        /**
         * Lấy thông tin thanh toán bằng OrderId.
         */
        Task<Payment?> GetPaymentByOrderIdAsync(int orderId);

        /**
         * Lấy thông tin thanh toán bằng TransactionId (mã giao dịch từ cổng TT).
         */
        Task<Payment?> GetPaymentByTransactionIdAsync(string transactionId);

        /**
         * Lấy tất cả các khoản thanh toán đang chờ xử lý.
         * Hữu ích cho các background job dọn dẹp.
         */
        Task<IEnumerable<Payment>> GetPendingPaymentsAsync();

        /**
         * (Tùy chọn) Xóa một bản ghi thanh toán (thường không nên làm).
         */
        Task<bool> DeletePaymentAsync(int paymentId);
    }
}
