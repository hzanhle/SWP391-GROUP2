using Stripe;
using Stripe.Checkout;

namespace BookingService.Services
{
    public interface IStripePaymentService
    {
        Task<Session> CreateCheckoutSessionAsync(
            decimal amount,
            string currency,
            string successUrl,
            string cancelUrl,
            Dictionary<string, string> metadata = null);

        Task<PaymentIntent> CreatePaymentIntentAsync(
            decimal amount,
            string currency,
            Dictionary<string, string> metadata = null);

        Task<Refund> CreateRefundAsync(string paymentIntentId, string reason = null);

        Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId);

        Task<Session> GetSessionAsync(string sessionId);
    }
}