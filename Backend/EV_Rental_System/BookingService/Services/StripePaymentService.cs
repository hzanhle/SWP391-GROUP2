using Stripe;
using Stripe.Checkout;

namespace BookingService.Services
{
    public class StripePaymentService : IStripePaymentService
    {
        private readonly string _secretKey;
        private readonly ILogger<StripePaymentService> _logger;

        public StripePaymentService(
            IConfiguration configuration,
            ILogger<StripePaymentService> logger)
        {
            _secretKey = configuration["Stripe:SecretKey"];
            _logger = logger;

            if (string.IsNullOrEmpty(_secretKey))
            {
                throw new InvalidOperationException("Stripe SecretKey is not configured");
            }

            StripeConfiguration.ApiKey = _secretKey;
        }

        /// <summary>
        /// Tạo Stripe Checkout Session
        /// Đây là cách đơn giản nhất, Stripe sẽ host trang thanh toán
        /// </summary>
        public async Task<Session> CreateCheckoutSessionAsync(
            decimal amount,
            string currency,
            string successUrl,
            string cancelUrl,
            Dictionary<string, string> metadata = null)
        {
            try
            {
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                Currency = currency.ToLower(),
                                // Stripe tính bằng đơn vị nhỏ nhất (cent cho USD, đồng cho VND)
                                UnitAmount = currency.ToLower() == "vnd"
                                    ? (long)amount
                                    : (long)(amount * 100),
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = "Thanh toán đơn hàng",
                                    Description = "Booking Service Payment"
                                }
                            },
                            Quantity = 1
                        }
                    },
                    Mode = "payment",
                    SuccessUrl = successUrl,
                    CancelUrl = cancelUrl,
                    Metadata = metadata ?? new Dictionary<string, string>(),
                    // Thêm các options hữu ích
                    CustomerEmail = null, // Có thể set email khách hàng nếu có
                    ExpiresAt = DateTime.UtcNow.AddMinutes(30), // Session hết hạn sau 30 phút
                    PaymentIntentData = new SessionPaymentIntentDataOptions
                    {
                        Metadata = metadata ?? new Dictionary<string, string>()
                    }
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options);

                _logger.LogInformation(
                    "Stripe Checkout Session created - SessionId: {SessionId}, Amount: {Amount} {Currency}",
                    session.Id, amount, currency.ToUpper()
                );

                return session;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating checkout session");
                throw new InvalidOperationException($"Lỗi tạo checkout session: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Tạo Payment Intent
        /// Dùng khi bạn muốn tự custom UI thanh toán
        /// </summary>
        public async Task<PaymentIntent> CreatePaymentIntentAsync(
            decimal amount,
            string currency,
            Dictionary<string, string> metadata = null)
        {
            try
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = currency.ToLower() == "vnd"
                        ? (long)amount
                        : (long)(amount * 100),
                    Currency = currency.ToLower(),
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true
                    },
                    Metadata = metadata ?? new Dictionary<string, string>()
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);

                _logger.LogInformation(
                    "Stripe Payment Intent created - PaymentIntentId: {PaymentIntentId}, Amount: {Amount} {Currency}",
                    paymentIntent.Id, amount, currency.ToUpper()
                );

                return paymentIntent;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating payment intent");
                throw new InvalidOperationException($"Lỗi tạo payment intent: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Tạo Refund (hoàn tiền)
        /// </summary>
        public async Task<Refund> CreateRefundAsync(string paymentIntentId, string reason = null)
        {
            try
            {
                var options = new RefundCreateOptions
                {
                    PaymentIntent = paymentIntentId,
                    Reason = reason switch
                    {
                        "duplicate" => "duplicate",
                        "fraudulent" => "fraudulent",
                        _ => "requested_by_customer"
                    }
                };

                var service = new RefundService();
                var refund = await service.CreateAsync(options);

                _logger.LogInformation(
                    "Stripe Refund created - RefundId: {RefundId}, PaymentIntentId: {PaymentIntentId}, Amount: {Amount}",
                    refund.Id, paymentIntentId, refund.Amount / 100m
                );

                return refund;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating refund for PaymentIntent {PaymentIntentId}", paymentIntentId);
                throw new InvalidOperationException($"Lỗi tạo refund: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Lấy thông tin Payment Intent
        /// </summary>
        public async Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId)
        {
            try
            {
                var service = new PaymentIntentService();
                return await service.GetAsync(paymentIntentId);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error getting payment intent {PaymentIntentId}", paymentIntentId);
                throw new InvalidOperationException($"Lỗi lấy payment intent: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Lấy thông tin Checkout Session
        /// </summary>
        public async Task<Session> GetSessionAsync(string sessionId)
        {
            try
            {
                var service = new SessionService();
                return await service.GetAsync(sessionId);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error getting session {SessionId}", sessionId);
                throw new InvalidOperationException($"Lỗi lấy session: {ex.Message}", ex);
            }
        }
    }
}