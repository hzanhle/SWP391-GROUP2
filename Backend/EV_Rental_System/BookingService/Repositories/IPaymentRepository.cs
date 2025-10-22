using BookingService.Models;

namespace BookingService.Repositories
{
    public interface IPaymentRepository
    {
        Task<int> CreateAsync(Payment payment);
        Task<Payment?> GetByIdAsync(int paymentId);
        Task<Payment?> GetByOrderIdAsync(int orderId);
        Task<Payment?> GetByTransactionIdAsync(string transactionId);
        Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status);
        Task<IEnumerable<Payment>> GetByPaymentMethodAsync(string paymentMethod);
        Task<IEnumerable<Payment>> GetPendingPaymentsAsync();
        Task<bool> UpdateAsync(Payment payment);
        Task<bool> DeleteAsync(int paymentId);
        Task<bool> ExistsByOrderIdAsync(int orderId);
    }
}
