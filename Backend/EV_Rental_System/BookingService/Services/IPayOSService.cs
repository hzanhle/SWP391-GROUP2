using System.Threading.Tasks;

namespace BookingService.Services
{
    public interface IPayOSService
    {
        Task<(bool Success, string? CheckoutUrl, string? PaymentLinkId, string? Error)> CreatePaymentLinkAsync(int orderId, decimal amount, string description);
        Task<(bool Success, string? RefundId, string? Error)> RefundDepositAsync(string transactionId, decimal amount, string reason);
        Task<(bool Success, string? PayoutId, string? Error)> CreatePayoutAsync(string transactionId, decimal amount, string description, string? accountNumber = null, string? accountName = null, string? bankCode = null);
        bool ValidateWebhookSignature(string payload, string signature);
    }
}
