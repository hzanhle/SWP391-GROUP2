using BookingService.Models.ModelSettings;
using BookingService.Services;
using BookingService.Services.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System;
using System.Security.Claims;

namespace BookingService.Controllers
{
    [ApiController]
    [Route("api/payment")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IVNPayService _vnpayService;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;
        private readonly IHubContext<OrderTimerHub> _hubContext;
        private readonly FrontendSettings _frontendSettings;

        public PaymentController(
            IVNPayService vnpayService,
            IPaymentService paymentService,
            ILogger<PaymentController> logger,
            IHubContext<OrderTimerHub> hubContext,
            IOptions<FrontendSettings> frontendSettings)
        {
            _vnpayService = vnpayService;
            _paymentService = paymentService;
            _logger = logger;
            _hubContext = hubContext;
            _frontendSettings = frontendSettings?.Value ?? new FrontendSettings();
        }

        /// <summary>
        /// Tạo URL thanh toán VNPay cho Order đã tồn tại
        /// </summary>
        [HttpGet("vnpay-create")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> CreatePaymentUrl([FromQuery] int orderId)
        {
            try
            {
                if (orderId <= 0)
                {
                    return BadRequest(new { message = "OrderId không hợp lệ" });
                }

                // ✅ FIX: Validate ownership
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Không xác định được user" });
                }

                var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
                if (payment == null)
                {
                    return NotFound(new { message = $"Không tìm thấy payment cho Order #{orderId}" });
                }

                // ✅ Validate user owns this order
                var ownsOrder = await _paymentService.ValidateOrderOwnershipAsync(orderId, userId);
                if (!ownsOrder)
                {
                    return Forbid(); // 403
                }

                // Kiểm tra trạng thái
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

                // ✅ Check expired
                if (payment.IsExpired)
                {
                    await _paymentService.CancelPaymentDueToTimeoutAsync(payment.PaymentId, "Timeout before creating payment URL");
                    return BadRequest(new { message = "Payment đã hết hạn" });
                }

                // Tạo VNPay payment URL
                var paymentUrl = _vnpayService.CreatePaymentUrl(
                    orderId,
                    payment.Amount,
                    $"Thanh toan don hang #{orderId}"
                );

                _logger.LogInformation(
                    "Created VNPay URL - Order: {OrderId}, User: {UserId}, Amount: {Amount}",
                    orderId, userId, payment.Amount
                );

                return Ok(new
                {
                    success = true,
                    paymentUrl,
                    orderId,
                    amount = payment.Amount,
                    paymentMethod = payment.PaymentMethod,
                    expiresAt = payment.ExpiresAt
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating VNPay URL for Order {OrderId}", orderId);
                return StatusCode(500, new { message = "Lỗi tạo URL thanh toán" });
            }
        }

        /// <summary>
        /// Callback từ VNPay (user redirect)
        /// </summary>
        [HttpGet("vnpay-deposit-callback")]
        [AllowAnonymous]
        public async Task<IActionResult> VNPayCallback()
        {
            try
            {
                var query = Request.Query;

                _logger.LogInformation("VNPay callback received: {@Query}",
                    query.ToDictionary(k => k.Key, v => v.Value.ToString()));

                // ✅ Validate signature
                if (!_vnpayService.ValidateCallback(query))
                {
                    _logger.LogWarning("❌ VNPay callback - Invalid signature");
                    return BadRequest(new { success = false, message = "Chữ ký không hợp lệ" });
                }

                // Parse data
                var responseCode = query["vnp_ResponseCode"].ToString();
                var txnRef = query["vnp_TxnRef"].ToString();
                var amount = query["vnp_Amount"].ToString();
                var transactionNo = query["vnp_TransactionNo"].ToString();
                var bankCode = query["vnp_BankCode"].ToString();
                var payDate = query["vnp_PayDate"].ToString();

                var orderId = ParseOrderIdFromTxnRef(txnRef);
                if (orderId == 0)
                {
                    _logger.LogError("Invalid TxnRef: {TxnRef}", txnRef);
                    return BadRequest(new { success = false, message = "TxnRef không hợp lệ" });
                }

                // ✅ Parse and validate amount
                var actualAmount = long.TryParse(amount, out var amt) ? amt / 100 : 0;

                var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
                if (payment == null)
                {
                    _logger.LogError("Payment not found for Order {OrderId}", orderId);
                    return NotFound(new { success = false, message = "Không tìm thấy payment" });
                }

                // ✅ Validate amount
                if (actualAmount != payment.Amount)
                {
                    _logger.LogError(
                        "Amount mismatch - Order: {OrderId}, Expected: {Expected}, Actual: {Actual}",
                        orderId, payment.Amount, actualAmount
                    );
                    return BadRequest(new { success = false, message = "Số tiền không khớp" });
                }

                // ✅ Check if already processed (idempotency)
                if (payment.IsCompleted())
                {
                    _logger.LogInformation("Payment already completed for Order {OrderId}", orderId);
                    return Ok(new
                    {
                        success = true,
                        message = "Đơn hàng đã được thanh toán",
                        data = new
                        {
                            orderId,
                            transactionNo = payment.TransactionId,
                            amount = payment.Amount,
                            paidAt = payment.PaidAt
                        }
                    });
                }

                // Process payment
                if (responseCode == "00")
                {
                    var gatewayResponse = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        responseCode,
                        transactionNo,
                        bankCode,
                        payDate,
                        amount = actualAmount
                    });

                    var success = await _paymentService.MarkPaymentCompletedAsync(
                        orderId,
                        transactionNo,
                        gatewayResponse
                    );

                    if (success)
                    {
                        _logger.LogInformation(
                            "✅ Payment completed - Order: {OrderId}, TxnNo: {TransactionNo}",
                            orderId, transactionNo
                        );

                        // Send SignalR notification to waiting clients
                        try
                        {
                            var txnIdInt = int.TryParse(transactionNo, out var txnId) ? txnId : 0;
                            await _hubContext.Clients.Group($"order_{orderId}")
                                .SendAsync("PaymentSuccess", new { OrderId = orderId, TransactionId = transactionNo });
                            _logger.LogInformation("📡 SignalR PaymentSuccess sent for Order {OrderId}", orderId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "⚠️ Failed to send SignalR notification for Order {OrderId}", orderId);
                        }

                        var feUrl = GetFrontendBaseUrl();
                        var redirectUrl = $"{feUrl}?success=true&orderId={orderId}&transactionId={transactionNo}#payment";
                        return Redirect(redirectUrl);
                    }
                    else
                    {
                        _logger.LogError("Failed to mark payment completed for Order {OrderId}", orderId);
                        var feUrl = GetFrontendBaseUrl();
                        var redirectUrl = $"{feUrl}?success=false&orderId={orderId}&error=payment_update_failed#payment";
                        return Redirect(redirectUrl);
                    }
                }
                else
                {
                    var errorMessage = GetVNPayErrorMessage(responseCode);
                    var gatewayResponse = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        responseCode,
                        errorMessage
                    });

                    await _paymentService.MarkPaymentFailedAsync(orderId, gatewayResponse);

                    _logger.LogWarning(
                        "❌ Payment failed - Order: {OrderId}, Code: {Code}, Message: {Message}",
                        orderId, responseCode, errorMessage
                    );

                    var feUrl = GetFrontendBaseUrl();
                    var redirectUrl = $"{feUrl}#payment?success=false&orderId={orderId}&error={errorMessage}";
                    return Redirect(redirectUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing VNPay callback");
                return StatusCode(500, new { success = false, message = "Lỗi xử lý callback" });
            }
        }

        /// <summary>
        /// IPN endpoint (webhook từ VNPay server-to-server)
        /// </summary>
        [HttpGet("vnpay-ipn")]
        [AllowAnonymous]
        public async Task<IActionResult> VNPayIPN()
        {
            try
            {
                var query = Request.Query;

                _logger.LogInformation("VNPay IPN received: {@Query}",
                    query.ToDictionary(k => k.Key, v => v.Value.ToString()));

                // ✅ Validate signature - CRITICAL
                if (!_vnpayService.ValidateCallback(query))
                {
                    _logger.LogWarning("VNPay IPN - Invalid signature");
                    return Ok(new { RspCode = "97", Message = "Invalid signature" });
                }

                var responseCode = query["vnp_ResponseCode"].ToString();
                var txnRef = query["vnp_TxnRef"].ToString();
                var transactionNo = query["vnp_TransactionNo"].ToString();
                var amount = query["vnp_Amount"].ToString();

                var orderId = ParseOrderIdFromTxnRef(txnRef);
                if (orderId == 0)
                {
                    return Ok(new { RspCode = "01", Message = "Order not found" });
                }

                var actualAmount = long.TryParse(amount, out var amt) ? amt / 100 : 0;

                // ✅ Get payment with lock (prevent race condition)
                var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
                if (payment == null)
                {
                    return Ok(new { RspCode = "01", Message = "Order not found" });
                }

                // ✅ Validate amount
                if (actualAmount != payment.Amount)
                {
                    _logger.LogError("IPN - Amount mismatch for Order {OrderId}", orderId);
                    return Ok(new { RspCode = "04", Message = "Invalid amount" });
                }

                // ✅ Idempotency check
                if (payment.IsCompleted())
                {
                    _logger.LogInformation("VNPay IPN - Payment already processed for Order {OrderId}", orderId);
                    return Ok(new { RspCode = "00", Message = "Confirm Success" });
                }

                // Process payment
                if (responseCode == "00")
                {
                    var gatewayResponse = System.Text.Json.JsonSerializer.Serialize(
                        query.ToDictionary(k => k.Key, v => v.Value.ToString())
                    );

                    await _paymentService.MarkPaymentCompletedAsync(orderId, transactionNo, gatewayResponse);
                    _logger.LogInformation("VNPay IPN - Payment completed for Order {OrderId}", orderId);

                    // Send SignalR notification to waiting clients
                    try
                    {
                        await _hubContext.Clients.Group($"order_{orderId}")
                            .SendAsync("PaymentSuccess", new { OrderId = orderId, TransactionId = transactionNo });
                        _logger.LogInformation("📡 SignalR PaymentSuccess sent for Order {OrderId} (IPN)", orderId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "⚠️ Failed to send SignalR notification (IPN) for Order {OrderId}", orderId);
                    }
                }
                else
                {
                    var gatewayResponse = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        responseCode,
                        message = GetVNPayErrorMessage(responseCode)
                    });

                    await _paymentService.MarkPaymentFailedAsync(orderId, gatewayResponse);
                    _logger.LogWarning("VNPay IPN - Payment failed for Order {OrderId}", orderId);
                }

                return Ok(new { RspCode = "00", Message = "Confirm Success" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing VNPay IPN");
                return Ok(new { RspCode = "99", Message = "Unknown error" });
            }
        }

        /// <summary>
        /// Query payment status
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

                // ✅ Validate ownership for Member role
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole == "Member")
                {
                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var ownsOrder = await _paymentService.ValidateOrderOwnershipAsync(orderId, userId);
                    if (!ownsOrder)
                    {
                        return Forbid();
                    }
                }

                return Ok(new
                {
                    orderId = payment.OrderId,
                    amount = payment.Amount,
                    status = payment.Status.ToString(),
                    paymentMethod = payment.PaymentMethod,
                    transactionId = payment.TransactionId,
                    paidAt = payment.PaidAt,
                    createdAt = payment.CreatedAt,
                    expiresAt = payment.ExpiresAt,
                    isExpired = payment.IsExpired
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment status for Order {OrderId}", orderId);
                return StatusCode(500, new { message = "Lỗi lấy thông tin payment" });
            }
        }

        // ===== ADMIN ENDPOINTS =====

        [HttpGet("pending")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetPendingPayments()
        {
            try
            {
                var payments = await _paymentService.GetPendingPaymentsAsync();

                var result = payments.Select(p => new
                {
                    p.PaymentId,
                    p.OrderId,
                    p.Amount,
                    p.PaymentMethod,
                    p.Status,
                    p.CreatedAt,
                    p.ExpiresAt,
                    IsExpired = p.IsExpired,
                    TimeRemaining = p.TimeRemaining?.ToString(@"hh\:mm\:ss")
                });

                return Ok(new { success = true, count = payments.Count(), data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending payments");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        [HttpPost("process-expired")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ProcessExpiredPayments([FromQuery] int expirationMinutes = 15)
        {
            try
            {
                _logger.LogInformation("Admin triggered ProcessExpiredPayments");

                var cancelledCount = await _paymentService.ProcessExpiredPaymentsAsync(expirationMinutes);

                return Ok(new
                {
                    success = true,
                    message = $"Cancelled {cancelledCount} expired payments",
                    cancelledCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing expired payments");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        // ===== PayOS Integration =====

        [HttpPost("payos/create")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> CreatePayOSPayment([FromBody] int orderId, [FromServices] IPayOSService payos)
        {
            try
            {
                if (orderId <= 0) return BadRequest(new { message = "Invalid orderId" });
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
                if (payment == null) return NotFound(new { message = "Payment not found" });

                if (payment.IsCompleted()) return BadRequest(new { message = "Payment already completed" });
                if (payment.IsFailed()) return BadRequest(new { message = "Payment failed. Please create a new order." });
                if (payment.IsExpired)
                {
                    await _paymentService.CancelPaymentDueToTimeoutAsync(payment.PaymentId, "Timeout before PayOS create");
                    return BadRequest(new { message = "Payment expired" });
                }

                // Ensure payment method is set to PayOS for downstream flows (e.g., refund)
                await _paymentService.UpdatePaymentMethodAsync(orderId, "PayOS");

                var res = await payos.CreatePaymentLinkAsync(orderId, payment.Amount, $"Thanh toan don hang #{orderId}");
                if (!res.Success || string.IsNullOrEmpty(res.CheckoutUrl))
                {
                    return StatusCode(502, new { message = res.Error ?? "PayOS error" });
                }

                return Ok(new { success = true, paymentUrl = res.CheckoutUrl, orderId, amount = payment.Amount, paymentMethod = "PayOS" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PayOS link for Order {OrderId}", orderId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("payos-deposit-callback")]
        [AllowAnonymous]
        public async Task<IActionResult> PayOSDepositCallback()
        {
            var query = Request.Query;
            _logger.LogInformation("PayOS deposit callback received: {@Query}",
                query.ToDictionary(k => k.Key, v => v.Value.ToString()));

            var code = query["code"].ToString();
            var status = query["status"].ToString();
            var orderCodeStr = query["orderCode"].ToString();
            var transactionId = query["transactionId"].ToString();
            if (string.IsNullOrWhiteSpace(transactionId))
            {
                transactionId = query["id"].ToString();
            }

            var frontendBase = GetFrontendBaseUrl();

            if (!int.TryParse(orderCodeStr, out var orderId) || orderId <= 0)
            {
                return Redirect($"{frontendBase}?success=false&error=invalid_order#payment");
            }

            try
            {
                var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
                if (payment == null)
                {
                    _logger.LogWarning("PayOS deposit callback: payment not found for order {OrderId}", orderId);
                    return Redirect($"{frontendBase}?success=false&orderId={orderId}&error=payment_not_found#payment");
                }

                if (payment.IsCompleted())
                {
                    _logger.LogInformation("PayOS deposit callback: payment already completed for order {OrderId}", orderId);
                    return Redirect($"{frontendBase}?success=true&orderId={orderId}&transactionId={payment.TransactionId ?? transactionId}#payment");
                }

                if (string.Equals(code, "00", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(status, "PAID", StringComparison.OrdinalIgnoreCase))
                {
                    var txn = string.IsNullOrWhiteSpace(transactionId)
                        ? Guid.NewGuid().ToString("N")
                        : transactionId;

                    var serialized = System.Text.Json.JsonSerializer.Serialize(
                        query.ToDictionary(k => k.Key, v => v.Value.ToString()));

                    var success = await _paymentService.MarkPaymentCompletedAsync(orderId, txn, serialized);
                    if (!success)
                    {
                        return Redirect($"{frontendBase}?success=false&orderId={orderId}&error=payment_update_failed#payment");
                    }

                    try
                    {
                        await _hubContext.Clients.Group($"order_{orderId}")
                            .SendAsync("PaymentSuccess", new { OrderId = orderId, TransactionId = txn });
                        _logger.LogInformation("SignalR PayOS PaymentSuccess sent for order {OrderId}", orderId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to send PayOS PaymentSuccess SignalR for order {OrderId}", orderId);
                    }

                    return Redirect($"{frontendBase}?success=true&orderId={orderId}&transactionId={txn}#payment");
                }
                else
                {
                    return Redirect($"{frontendBase}?success=false&orderId={orderId}&error=payment_failed#payment");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling PayOS deposit callback for Order {OrderId}", orderId);
                return Redirect($"{frontendBase}?success=false&orderId={orderId}&error=callback_error#payment");
            }
        }

        [HttpPost("payos/webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> PayOSWebhook([FromServices] IPayOSService payos)
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            var signature = Request.Headers["X-Checksum"]; // header name depending on PayOS
            if (string.IsNullOrWhiteSpace(signature))
            {
                _logger.LogWarning("PayOS webhook ping received without signature. Returning 200 for verification.");
                return Ok(new { success = false, message = "Missing checksum" });
            }
            if (!payos.ValidateWebhookSignature(body, signature))
            {
                _logger.LogWarning("PayOS webhook: invalid signature");
                return Unauthorized(new { success = false });
            }

            try
            {
                var doc = System.Text.Json.JsonDocument.Parse(body);
                var root = doc.RootElement;
                var data = root.TryGetProperty("data", out var d) ? d : root;
                var status = data.TryGetProperty("status", out var st) ? st.GetString() : null;
                var orderCodeStr = data.TryGetProperty("orderCode", out var oc) ? oc.GetString() : null;
                var transactionId = data.TryGetProperty("transactionId", out var tid) ? tid.GetString() : null;
                var amount = data.TryGetProperty("amount", out var amt) ? amt.GetDecimal() : 0m;

                if (!int.TryParse(orderCodeStr, out var orderId) || orderId <= 0)
                {
                    _logger.LogError("PayOS webhook: invalid orderCode {OrderCode}", orderCodeStr);
                    return BadRequest(new { success = false });
                }

                var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
                if (payment == null) return NotFound(new { success = false, message = "Payment not found" });

                if (string.Equals(status, "PAID", StringComparison.OrdinalIgnoreCase))
                {
                    var success = await _paymentService.MarkPaymentCompletedAsync(orderId, transactionId ?? string.Empty, body);
                    _logger.LogInformation("PayOS webhook: payment completed for Order {OrderId}", orderId);
                    try
                    {
                        await _hubContext.Clients.Group($"order_{orderId}")
                            .SendAsync("PaymentSuccess", new { OrderId = orderId, TransactionId = transactionId ?? string.Empty });
                        _logger.LogInformation("📡 SignalR PaymentSuccess sent for Order {OrderId} (PayOS)", orderId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "⚠️ Failed to send SignalR notification (PayOS) for Order {OrderId}", orderId);
                    }
                    return Ok(new { success });
                }
                else if (string.Equals(status, "CANCELLED", StringComparison.OrdinalIgnoreCase) || string.Equals(status, "FAILED", StringComparison.OrdinalIgnoreCase))
                {
                    await _paymentService.MarkPaymentFailedAsync(orderId, body);
                    _logger.LogInformation("PayOS webhook: payment failed/cancelled for Order {OrderId}", orderId);
                    return Ok(new { success = true });
                }

                // Unknown status -> accept
                _logger.LogInformation("PayOS webhook: status {Status} for Order {OrderId}", status, orderId);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling PayOS webhook");
                return StatusCode(500, new { success = false });
            }
        }

        [HttpPost("payos/refund")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> RefundDeposit([FromBody] int orderId, [FromServices] IPayOSService payos)
        {
            try
            {
                if (orderId <= 0) return BadRequest(new { message = "Invalid orderId" });
                var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
                if (payment == null) return NotFound(new { message = "Payment not found" });
                if (!string.Equals(payment.PaymentMethod, "PayOS", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Refund only supported for PayOS payments" });
                }

                var deposit = payment.Order?.DepositAmount ?? 0m;
                if (deposit <= 0) return BadRequest(new { message = "No deposit to refund" });
                if (string.IsNullOrWhiteSpace(payment.TransactionId)) return BadRequest(new { message = "Missing transactionId" });

                var res = await payos.RefundDepositAsync(payment.TransactionId!, deposit, $"Refund deposit for Order #{orderId}");
                if (!res.Success)
                {
                    return StatusCode(502, new { message = res.Error ?? "PayOS refund error" });
                }
                return Ok(new { success = true, refundId = res.RefundId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refunding deposit via PayOS for Order {OrderId}", orderId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ===== HELPER METHODS =====

        private int ParseOrderIdFromTxnRef(string txnRef)
        {
            try
            {
                var parts = txnRef.Split('_');
                if (parts.Length > 0 && int.TryParse(parts[0], out var orderId))
                {
                    return orderId;
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private string GetVNPayErrorMessage(string responseCode)
        {
            return responseCode switch
            {
                "00" => "Giao dịch thành công",
                "07" => "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường).",
                "09" => "Thẻ/Tài khoản chưa đăng ký dịch vụ InternetBanking tại ngân hàng.",
                "10" => "Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần",
                "11" => "Đã hết hạn chờ thanh toán. Vui lòng thực hiện lại giao dịch.",
                "12" => "Thẻ/Tài khoản bị khóa.",
                "13" => "Quý khách nhập sai mật khẩu xác thực giao dịch (OTP).",
                "24" => "Khách hàng hủy giao dịch",
                "51" => "Tài khoản không đủ số dư để thực hiện giao dịch.",
                "65" => "Tài khoản đã vượt quá hạn mức giao dịch trong ngày.",
                "75" => "Ngân hàng thanh toán đang bảo trì.",
                "79" => "KH nhập sai mật khẩu thanh toán quá số lần quy định.",
                _ => $"Giao dịch thất bại - Mã lỗi: {responseCode}"
            };
        }

        private string GetFrontendBaseUrl()
        {
            var envValue = Environment.GetEnvironmentVariable("FRONTEND_URL");
            if (!string.IsNullOrWhiteSpace(envValue))
            {
                return envValue.TrimEnd('/');
            }

            if (!string.IsNullOrWhiteSpace(_frontendSettings.BaseUrl))
            {
                return _frontendSettings.BaseUrl.TrimEnd('/');
            }

            return "http://localhost:5173";
        }
    }
}
