using BookingService.Models;
namespace BookingService.Services
{
    public interface IPaymentService
    {
        // Core operations
        Task<Payment> CreatePaymentForOrderAsync(int orderId, decimal amount, string paymentMethod);
        Task<bool> MarkPaymentCompletedAsync(int orderId, string transactionId, string? gatewayResponse);
        Task<bool> MarkPaymentFailedAsync(int orderId, string? gatewayResponse);
        Task<bool> MarkPaymentCancelledAsync(int orderId, string? reason);
        Task<bool> UpdatePaymentMethodAsync(int orderId, string paymentMethod);

        // Query operations
        Task<Payment?> GetPaymentByOrderIdAsync(int orderId);
        Task<Payment?> GetPaymentByTransactionIdAsync(string transactionId);
        Task<IEnumerable<Payment>> GetPendingPaymentsAsync();
        Task<IEnumerable<Payment>> GetExpiredPendingPaymentsAsync(int? expirationMinutes = null);

        // Background jobs
        Task<bool> CancelPaymentDueToTimeoutAsync(int paymentId, string reason = "Payment timeout");
        Task<int> ProcessExpiredPaymentsAsync(int? expirationMinutes = null);

        // Gateway sync
        Task<bool> SyncPaymentStatusFromGatewayAsync(int paymentId);
        Task<int> SyncAllPendingPaymentsAsync(int minAgeMinutes = 5);

        // Validation
        Task<bool> ValidateOrderOwnershipAsync(int orderId, string userId);

        // Admin
        Task<bool> DeletePaymentAsync(int paymentId);
    }
}
