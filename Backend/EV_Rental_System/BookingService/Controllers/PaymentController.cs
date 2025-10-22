using BookingService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;


namespace BookingService.Controllers
{
    [ApiController]
    [Route("api/payment")]
    [Authorize] // Yêu cầu authentication cho tất cả endpoints
    public class PaymentController : ControllerBase
    {
        private readonly IStripePaymentService _stripeService;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;
        private readonly IConfiguration _configuration;

        public PaymentController(
            IStripePaymentService stripeService,
            IPaymentService paymentService,
            ILogger<PaymentController> logger,
            IConfiguration configuration)
        {
            _stripeService = stripeService;
            _paymentService = paymentService;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Tạo Stripe Checkout Session cho Order đã tồn tại
        /// POST: api/payment/create-checkout-session
        /// Body: { "orderId": 123 }
        /// Chỉ Member (khách hàng) mới được tạo checkout session
        /// </summary>
        [HttpPost("create-checkout-session")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutRequest request)
        {
            try
            {
                if (request.OrderId <= 0)
                {
                    return BadRequest(new { message = "OrderId không hợp lệ" });
                }

                // Lấy payment record từ database
                var payment = await _paymentService.GetPaymentByOrderIdAsync(request.OrderId);

                if (payment == null)
                {
                    return NotFound(new { message = $"Không tìm thấy payment cho Order #{request.OrderId}" });
                }

                // TODO: Service phải validate Member chỉ tạo payment cho đơn hàng của mình
                // var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                // await _paymentService.ValidateOrderOwnershipAsync(request.OrderId, userId);

                // Kiểm tra trạng thái payment
                if (payment.IsCompleted())
                {
                    return BadRequest(new
                    {
                        message = "Payment đã được thanh toán",
                        transactionId = payment.TransactionId,
                        paidAt = payment.PaidAt
                    });
                }

                if (payment.IsFailed())
                {
                    return BadRequest(new { message = "Payment đã thất bại, vui lòng tạo order mới" });
                }

                // Tạo Stripe Checkout Session
                var domain = $"{Request.Scheme}://{Request.Host}";
                var session = await _stripeService.CreateCheckoutSessionAsync(
                    payment.Amount,
                    "vnd", // hoặc "usd" tùy theo currency của bạn
                    $"{domain}/api/payment/success?orderId={request.OrderId}",
                    $"{domain}/api/payment/cancel?orderId={request.OrderId}",
                    new Dictionary<string, string>
                    {
                        { "order_id", request.OrderId.ToString() }
                    }
                );

                // Domain sẽ tự động là http://localhost:5049 khi chạy local

                _logger.LogInformation(
                    "Tạo Stripe Checkout Session cho Order {OrderId}, SessionId: {SessionId}, Amount: {Amount}",
                    request.OrderId, session.Id, payment.Amount
                );

                return Ok(new
                {
                    success = true,
                    sessionId = session.Id,
                    checkoutUrl = session.Url,
                    orderId = request.OrderId,
                    amount = payment.Amount,
                    paymentMethod = payment.PaymentMethod
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi tạo Stripe Checkout Session cho Order {OrderId}", request.OrderId);
                return StatusCode(500, new { message = "Lỗi tạo checkout session", error = ex.Message });
            }
        }

        /// <summary>
        /// Success callback từ Stripe (user redirect)
        /// GET: api/payment/success?orderId=123
        /// AllowAnonymous vì đây là callback từ Stripe redirect browser
        /// </summary>
        [HttpGet("success")]
        [AllowAnonymous]
        public async Task<IActionResult> PaymentSuccess([FromQuery] int orderId)
        {
            try
            {
                _logger.LogInformation("Payment success callback - Order: {OrderId}", orderId);

                var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);

                if (payment == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy payment" });
                }

                // Stripe webhook sẽ xử lý việc cập nhật payment status
                // Endpoint này chỉ để hiển thị thông báo cho user
                return Ok(new
                {
                    success = true,
                    message = "Thanh toán thành công! Đang xử lý đơn hàng của bạn.",
                    orderId,
                    status = payment.Status.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xử lý success callback cho Order {OrderId}", orderId);
                return StatusCode(500, new { success = false, message = "Lỗi xử lý callback" });
            }
        }

        /// <summary>
        /// Cancel callback từ Stripe (user redirect)
        /// GET: api/payment/cancel?orderId=123
        /// AllowAnonymous vì đây là callback từ Stripe redirect browser
        /// </summary>
        [HttpGet("cancel")]
        [AllowAnonymous]
        public IActionResult PaymentCancel([FromQuery] int orderId)
        {
            _logger.LogInformation("Payment cancelled - Order: {OrderId}", orderId);

            return Ok(new
            {
                success = false,
                message = "Thanh toán đã bị hủy",
                orderId
            });
        }

        /// <summary>
        /// Webhook endpoint cho Stripe (server-to-server)
        /// POST: api/payment/stripe-webhook
        /// AllowAnonymous vì webhook từ Stripe không có JWT token
        /// CRITICAL: PHẢI validate signature để đảm bảo request từ Stripe thật
        /// </summary>
        [HttpPost("stripe-webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> StripeWebhook()
        {
            try
            {
                var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                var stripeSignature = Request.Headers["Stripe-Signature"].ToString();
                var webhookSecret = _configuration["Stripe:WebhookSecret"];

                Event stripeEvent;

                try
                {
                    // Validate signature - CRITICAL SECURITY CHECK
                    stripeEvent = EventUtility.ConstructEvent(
                        json,
                        stripeSignature,
                        webhookSecret
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Stripe webhook - Invalid signature");
                    return BadRequest(new { error = "Invalid signature" });
                }

                _logger.LogInformation("Stripe webhook received: {EventType}", stripeEvent.Type);

                // Xử lý các event types
                switch (stripeEvent.Type)
                {
                    case "checkout.session.completed":
                        var session = stripeEvent.Data.Object as Session;
                        await HandleCheckoutSessionCompleted(session);
                        break;

                    case "payment_intent.succeeded":
                        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                        await HandlePaymentIntentSucceeded(paymentIntent);
                        break;

                    case "payment_intent.payment_failed":
                        var failedPaymentIntent = stripeEvent.Data.Object as PaymentIntent;
                        await HandlePaymentIntentFailed(failedPaymentIntent);
                        break;

                    case "charge.refunded":
                        var charge = stripeEvent.Data.Object as Charge;
                        await HandleChargeRefunded(charge);
                        break;

                    default:
                        _logger.LogInformation("Unhandled event type: {EventType}", stripeEvent.Type);
                        break;
                }

                return Ok(new { received = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xử lý Stripe webhook");
                return StatusCode(500, new { error = "Webhook handler failed" });
            }
        }

        /// <summary>
        /// Query payment status
        /// GET: api/payment/status/{orderId}
        /// Member xem status payment của đơn hàng mình
        /// Employee/Admin xem bất kỳ payment nào
        /// </summary>
        [HttpGet("status/{orderId}")]
        [Authorize(Roles = "Admin,Employee,Member")]
        public async Task<IActionResult> GetPaymentStatus(int orderId)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);

                if (payment == null)
                {
                    return NotFound(new { message = $"Không tìm thấy payment cho Order #{orderId}" });
                }

                // TODO: Service phải validate Member chỉ xem payment của đơn hàng mình
                // var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                // var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                // if (userRole == "Member") {
                //     await _paymentService.ValidateOrderOwnershipAsync(orderId, userId);
                // }

                return Ok(new
                {
                    orderId = payment.OrderId,
                    amount = payment.Amount,
                    status = payment.Status.ToString(),
                    paymentMethod = payment.PaymentMethod,
                    transactionId = payment.TransactionId,
                    paidAt = payment.PaidAt,
                    createdAt = payment.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi lấy payment status cho Order {OrderId}", orderId);
                return StatusCode(500, new { message = "Lỗi lấy thông tin payment" });
            }
        }

        /// <summary>
        /// Tạo refund cho một payment
        /// POST: api/payment/refund
        /// Body: { "orderId": 123, "reason": "Customer request" }
        /// Chỉ Admin/Employee được phép refund
        /// </summary>
        [HttpPost("refund")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> CreateRefund([FromBody] RefundRequest request)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByOrderIdAsync(request.OrderId);

                if (payment == null)
                {
                    return NotFound(new { message = $"Không tìm thấy payment cho Order #{request.OrderId}" });
                }

                if (!payment.IsCompleted())
                {
                    return BadRequest(new { message = "Chỉ có thể refund payment đã hoàn thành" });
                }

                if (string.IsNullOrEmpty(payment.TransactionId))
                {
                    return BadRequest(new { message = "Không tìm thấy transaction ID" });
                }

                // Tạo refund trên Stripe
                var refund = await _stripeService.CreateRefundAsync(
                    payment.TransactionId,
                    request.Reason
                );

                // Cập nhật database
                await _paymentService.MarkPaymentRefundedAsync(
                    request.OrderId,
                    refund.Id,
                    request.Reason
                );

                _logger.LogInformation(
                    "Refund created - Order: {OrderId}, RefundId: {RefundId}, Amount: {Amount}",
                    request.OrderId, refund.Id, refund.Amount / 100m
                );

                return Ok(new
                {
                    success = true,
                    message = "Refund thành công",
                    refundId = refund.Id,
                    amount = refund.Amount / 100m,
                    status = refund.Status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi tạo refund cho Order {OrderId}", request.OrderId);
                return StatusCode(500, new { message = "Lỗi tạo refund", error = ex.Message });
            }
        }

        // ===== PRIVATE HELPER METHODS =====

        private async Task HandleCheckoutSessionCompleted(Session session)
        {
            try
            {
                // Lấy orderId từ metadata
                if (session.Metadata.TryGetValue("order_id", out var orderIdStr) &&
                    int.TryParse(orderIdStr, out var orderId))
                {
                    var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);

                    if (payment != null && !payment.IsCompleted())
                    {
                        var gatewayResponse = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            sessionId = session.Id,
                            paymentStatus = session.PaymentStatus,
                            amountTotal = session.AmountTotal,
                            currency = session.Currency,
                            customerEmail = session.CustomerEmail
                        });

                        await _paymentService.MarkPaymentCompletedAsync(
                            orderId,
                            session.PaymentIntentId ?? session.Id,
                            gatewayResponse
                        );

                        _logger.LogInformation(
                            "✅ Checkout session completed - Order: {OrderId}, SessionId: {SessionId}",
                            orderId, session.Id
                        );

                        // TODO: Gọi OrderService để cập nhật Order status
                        // await _orderService.ConfirmPaymentAsync(orderId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling checkout.session.completed");
            }
        }

        private async Task HandlePaymentIntentSucceeded(PaymentIntent paymentIntent)
        {
            try
            {
                if (paymentIntent.Metadata.TryGetValue("order_id", out var orderIdStr) &&
                    int.TryParse(orderIdStr, out var orderId))
                {
                    var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);

                    if (payment != null && !payment.IsCompleted())
                    {
                        var gatewayResponse = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            paymentIntentId = paymentIntent.Id,
                            amount = paymentIntent.Amount,
                            currency = paymentIntent.Currency,
                            status = paymentIntent.Status
                        });

                        await _paymentService.MarkPaymentCompletedAsync(
                            orderId,
                            paymentIntent.Id,
                            gatewayResponse
                        );

                        _logger.LogInformation(
                            "✅ Payment intent succeeded - Order: {OrderId}, PaymentIntentId: {PaymentIntentId}",
                            orderId, paymentIntent.Id
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling payment_intent.succeeded");
            }
        }

        private async Task HandlePaymentIntentFailed(PaymentIntent paymentIntent)
        {
            try
            {
                if (paymentIntent.Metadata.TryGetValue("order_id", out var orderIdStr) &&
                    int.TryParse(orderIdStr, out var orderId))
                {
                    var gatewayResponse = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        paymentIntentId = paymentIntent.Id,
                        status = paymentIntent.Status,
                        lastPaymentError = paymentIntent.LastPaymentError?.Message
                    });

                    await _paymentService.MarkPaymentFailedAsync(orderId, gatewayResponse);

                    _logger.LogWarning(
                        "❌ Payment intent failed - Order: {OrderId}, Error: {Error}",
                        orderId, paymentIntent.LastPaymentError?.Message
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling payment_intent.payment_failed");
            }
        }

        private async Task HandleChargeRefunded(Charge charge)
        {
            try
            {
                // Tìm payment bằng PaymentIntentId
                // TODO: Implement logic tìm payment và cập nhật status
                _logger.LogInformation(
                    "Charge refunded - ChargeId: {ChargeId}, Amount: {Amount}",
                    charge.Id, charge.AmountRefunded / 100m
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling charge.refunded");
            }
        }
    }

    // ===== REQUEST/RESPONSE MODELS =====

    public class CreateCheckoutRequest
    {
        public int OrderId { get; set; }
    }

    public class RefundRequest
    {
        public int OrderId { get; set; }
        public string Reason { get; set; }
    }
}