using BookingService.Models;
using BookingService.Repositories;

namespace BookingService.Services
{

    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepo;
        private readonly ILogger<PaymentService> _logger;

        // Giả định UoW/DbContext được quản lý ở tầng cao hơn (OrderService)
        // và Repo sẽ không tự gọi SaveChanges().

        public PaymentService(
            IPaymentRepository paymentRepo,
            ILogger<PaymentService> logger)
        {
            _paymentRepo = paymentRepo;
            _logger = logger;
        }

        /**
         * Hàm này được OrderService gọi (trong 1 transaction)
         */
        public async Task<Payment> CreatePaymentForOrderAsync(int orderId, decimal amount, string paymentMethod)
        {
            if (await _paymentRepo.ExistsByOrderIdAsync(orderId))
            {
                throw new InvalidOperationException($"Payment record already exists for Order {orderId}");
            }

            // Dùng constructor của Payment, nó đã set Status = Pending
            var payment = new Payment(orderId, amount, paymentMethod);

            try
            {
                await _paymentRepo.CreateAsync(payment); // Chỉ Add vào DbContext
                _logger.LogInformation("Pending Payment {PaymentId} created for Order {OrderId}", payment.PaymentId, orderId);
                return payment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment for Order {OrderId}", orderId);
                throw;
            }
        }

        /**
         * Hàm này được Webhook Handler gọi (trong 1 transaction)
         */
        public async Task<bool> MarkPaymentCompletedAsync(int orderId, string transactionId, string? gatewayResponse)
        {
            var payment = await _paymentRepo.GetByOrderIdAsync(orderId);
            if (payment == null)
            {
                _logger.LogWarning("MarkPaymentCompleted: Payment not found for Order {OrderId}", orderId);
                return false;
            }

            try
            {
                // Sử dụng logic nghiệp vụ đã định nghĩa sẵn trong Model
                payment.MarkAsCompleted(transactionId, gatewayResponse);

                await _paymentRepo.UpdateAsync(payment); // Chỉ Update vào DbContext
                _logger.LogInformation("Payment for Order {OrderId} marked as COMPLETED. TxnId: {TransactionId}", orderId, transactionId);
                return true;
            }
            catch (InvalidOperationException ex)
            {
                // Lỗi nghiệp vụ (ví dụ: cố thanh toán 2 lần)
                _logger.LogWarning(ex, "MarkPaymentCompleted: Invalid operation for Order {OrderId}", orderId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking payment as completed for Order {OrderId}", orderId);
                throw;
            }
        }

        /**
         * Hàm này được Webhook Handler gọi (trong 1 transaction)
         */
        public async Task<bool> MarkPaymentFailedAsync(int orderId, string? gatewayResponse)
        {
            var payment = await _paymentRepo.GetByOrderIdAsync(orderId);
            if (payment == null)
            {
                _logger.LogWarning("MarkPaymentFailed: Payment not found for Order {OrderId}", orderId);
                return false;
            }

            try
            {
                // Sử dụng logic nghiệp vụ đã định nghĩa sẵn trong Model
                payment.MarkAsFailed(gatewayResponse);

                await _paymentRepo.UpdateAsync(payment); // Chỉ Update vào DbContext
                _logger.LogInformation("Payment for Order {OrderId} marked as FAILED.", orderId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking payment as failed for Order {OrderId}", orderId);
                throw;
            }
        }

        public async Task<Payment?> GetPaymentByOrderIdAsync(int orderId)
        {
            return await _paymentRepo.GetByOrderIdAsync(orderId);
        }

        public async Task<Payment?> GetPaymentByTransactionIdAsync(string transactionId)
        {
            return await _paymentRepo.GetByTransactionIdAsync(transactionId);
        }

        public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync()
        {
            return await _paymentRepo.GetPendingPaymentsAsync();
        }

        public async Task<bool> DeletePaymentAsync(int paymentId)
        {
            // Thận trọng khi dùng: Việc xóa payment thường là không nên.
            // Thay vào đó nên cập nhật trạng thái (ví dụ: Cancelled).
            try
            {
                _logger.LogWarning("Attempting to delete Payment {PaymentId}", paymentId);
                return await _paymentRepo.DeleteAsync(paymentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Payment {PaymentId}", paymentId);
                return false;
            }
        }
    }
}