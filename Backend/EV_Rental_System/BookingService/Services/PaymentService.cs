using BookingService.Models;
using BookingService.Repositories;

namespace BookingService.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepo;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IPaymentRepository paymentRepo,
            ILogger<PaymentService> logger)
        {
            _paymentRepo = paymentRepo;
            _logger = logger;
        }

        /// <summary>
        /// Tạo payment record với trạng thái Pending cho Order
        /// Được gọi bởi OrderService trong một transaction
        /// </summary>
        public async Task<Payment> CreatePaymentForOrderAsync(
            int orderId,
            decimal amount,
            string paymentMethod = "VNPay")
        {
            // Kiểm tra payment đã tồn tại chưa
            if (await _paymentRepo.ExistsByOrderIdAsync(orderId))
            {
                _logger.LogWarning("Payment already exists for Order {OrderId}", orderId);
                throw new InvalidOperationException($"Payment record already exists for Order {orderId}");
            }

            // Validate input
            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than 0", nameof(amount));
            }

            try
            {
                // Tạo Payment entity với constructor đầy đủ
                var payment = new Payment(orderId, amount, paymentMethod);

                // Lưu vào DbContext (không commit, để OrderService quản lý transaction)
                await _paymentRepo.CreateAsync(payment);

                _logger.LogInformation(
                    "Pending Payment created for Order {OrderId}. Amount: {Amount}, Method: {Method}",
                    orderId, amount, paymentMethod);

                return payment;
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Validation error creating payment for Order {OrderId}", orderId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating payment for Order {OrderId}", orderId);
                throw;
            }
        }

        /// <summary>
        /// Đánh dấu thanh toán thành công
        /// Được gọi bởi Webhook Handler hoặc OrderService.ConfirmPaymentAsync
        /// </summary>
        public async Task<bool> MarkPaymentCompletedAsync(
            int orderId,
            string transactionId,
            string? gatewayResponse = null)
        {
            if (string.IsNullOrWhiteSpace(transactionId))
            {
                throw new ArgumentException("TransactionId cannot be empty", nameof(transactionId));
            }

            var payment = await _paymentRepo.GetByOrderIdAsync(orderId);
            if (payment == null)
            {
                _logger.LogWarning("Payment not found for Order {OrderId}", orderId);
                return false;
            }

            // Kiểm tra trạng thái hiện tại
            if (payment.IsCompleted())
            {
                _logger.LogWarning(
                    "Payment for Order {OrderId} is already completed. TransactionId: {TransactionId}",
                    orderId, payment.TransactionId);
                return false; // Hoặc return true nếu idempotent
            }

            try
            {
                // Sử dụng domain method
                payment.MarkAsCompleted(transactionId, gatewayResponse);

                // Cập nhật vào DbContext
                await _paymentRepo.UpdateAsync(payment);

                _logger.LogInformation(
                    "Payment for Order {OrderId} marked as COMPLETED. TransactionId: {TransactionId}",
                    orderId, transactionId);

                return true;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation marking payment as completed for Order {OrderId}", orderId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking payment as completed for Order {OrderId}", orderId);
                throw;
            }
        }

        /// <summary>
        /// Đánh dấu thanh toán thất bại
        /// Được gọi bởi Webhook Handler
        /// </summary>
        public async Task<bool> MarkPaymentFailedAsync(int orderId, string? gatewayResponse = null)
        {
            var payment = await _paymentRepo.GetByOrderIdAsync(orderId);
            if (payment == null)
            {
                _logger.LogWarning("Payment not found for Order {OrderId}", orderId);
                return false;
            }

            // Không cho phép đánh dấu failed nếu đã completed
            if (payment.IsCompleted())
            {
                _logger.LogWarning(
                    "Cannot mark completed payment as failed for Order {OrderId}",
                    orderId);
                return false;
            }

            try
            {
                payment.MarkAsFailed(gatewayResponse);
                await _paymentRepo.UpdateAsync(payment);

                _logger.LogInformation("Payment for Order {OrderId} marked as FAILED", orderId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking payment as failed for Order {OrderId}", orderId);
                throw;
            }
        }

        /// <summary>
        /// Hoàn tiền (khi cancel order sau khi đã thanh toán)
        /// </summary>
        public async Task<bool> RefundPaymentAsync(int orderId, string? reason = null)
        {
            var payment = await _paymentRepo.GetByOrderIdAsync(orderId);
            if (payment == null)
            {
                _logger.LogWarning("Payment not found for Order {OrderId}", orderId);
                return false;
            }

            if (!payment.CanBeRefunded())
            {
                _logger.LogWarning(
                    "Payment for Order {OrderId} cannot be refunded. Current status: {Status}",
                    orderId, payment.Status);
                return false;
            }

            try
            {
                payment.MarkAsRefunded(reason);
                await _paymentRepo.UpdateAsync(payment);

                _logger.LogInformation(
                    "Payment for Order {OrderId} marked as REFUNDED. Reason: {Reason}",
                    orderId, reason ?? "N/A");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refunding payment for Order {OrderId}", orderId);
                throw;
            }
        }

        // ===== QUERY METHODS =====

        public async Task<Payment?> GetPaymentByOrderIdAsync(int orderId)
        {
            return await _paymentRepo.GetByOrderIdAsync(orderId);
        }

        public async Task<Payment?> GetPaymentByTransactionIdAsync(string transactionId)
        {
            if (string.IsNullOrWhiteSpace(transactionId))
                return null;

            return await _paymentRepo.GetByTransactionIdAsync(transactionId);
        }

        public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync()
        {
            return await _paymentRepo.GetPendingPaymentsAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(PaymentStatus status)
        {
            return await _paymentRepo.GetByStatusAsync(status);
        }

        /// <summary>
        /// Xóa payment - KHÔNG NÊN DÙNG trong production
        /// Chỉ dùng cho testing hoặc data cleanup
        /// </summary>
        public async Task<bool> DeletePaymentAsync(int paymentId)
        {
            try
            {
                _logger.LogWarning("DANGEROUS: Attempting to delete Payment {PaymentId}", paymentId);
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