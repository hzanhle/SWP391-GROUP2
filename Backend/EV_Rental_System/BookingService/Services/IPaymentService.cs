using BookingService.Models;
namespace BookingService.Services
{
    public interface IPaymentService
    {
        // Create
        Task<Payment> CreatePaymentForOrderAsync(int orderId, decimal amount, string paymentMethod = "Stripe");

        // Update Status
        Task<bool> MarkPaymentCompletedAsync(int orderId, string transactionId, string? gatewayResponse = null);
        Task<bool> MarkPaymentFailedAsync(int orderId, string? gatewayResponse = null);
        Task<bool> MarkPaymentRefundedAsync(int orderId, string refundId, string? reason = null);

        // Query
        Task<Payment?> GetPaymentByOrderIdAsync(int orderId);
        Task<Payment?> GetPaymentByTransactionIdAsync(string transactionId);
        Task<IEnumerable<Payment>> GetPendingPaymentsAsync();
        Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(PaymentStatus status);

        // Validation
        Task<bool> ValidateOrderOwnershipAsync(int orderId, string userId);
    }
}
