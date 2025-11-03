using BookingService.Services;
using BookingService.Services.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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

        public PaymentController(
            IVNPayService vnpayService,
            IPaymentService paymentService,
            ILogger<PaymentController> logger,
            IHubContext<OrderTimerHub> hubContext)
        {
            _vnpayService = vnpayService;
            _paymentService = paymentService;
            _logger = logger;
            _hubContext = hubContext;
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

                        // Get FE URL from environment or config
                        var feUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:5173";
                        var redirectUrl = $"{feUrl}?success=true&orderId={orderId}#payment";
                        return Redirect(redirectUrl);
                    }
                    else
                    {
                        _logger.LogError("Failed to mark payment completed for Order {OrderId}", orderId);
                        var feUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:5173";
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

                    var feUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:5173";
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
    }
}
