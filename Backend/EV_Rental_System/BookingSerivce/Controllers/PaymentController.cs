using Microsoft.AspNetCore.Mvc;
using BookingService.Services;
using BookingService.Models;

namespace BookingService.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : ControllerBase
    {
        private readonly IVNPayService _vnpayService;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IVNPayService vnpayService,
            IPaymentService paymentService,
            ILogger<PaymentController> logger)
        {
            _vnpayService = vnpayService;
            _paymentService = paymentService;
            _logger = logger;
        }

        /// <summary>
        /// Tạo URL thanh toán VNPay cho Order đã tồn tại
        /// GET: api/payment/vnpay-create?orderId=123
        /// </summary>
        [HttpGet("vnpay-create")]
        public async Task<IActionResult> CreatePaymentUrl([FromQuery] int orderId)
        {
            try
            {
                if (orderId <= 0)
                {
                    return BadRequest(new { message = "OrderId không hợp lệ" });
                }

                // Lấy payment record từ database
                var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);

                if (payment == null)
                {
                    return NotFound(new { message = $"Không tìm thấy payment cho Order #{orderId}" });
                }

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

                // Tạo VNPay payment URL
                var paymentUrl = _vnpayService.CreatePaymentUrl(
                    orderId,
                    payment.Amount,
                    $"Thanh toan don hang #{orderId}"
                );

                _logger.LogInformation(
                    "Tạo VNPay URL cho Order {OrderId}, Amount: {Amount}",
                    orderId, payment.Amount
                );

                return Ok(new
                {
                    success = true,
                    paymentUrl,
                    orderId,
                    amount = payment.Amount,
                    paymentMethod = payment.PaymentMethod
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi tạo VNPay URL cho Order {OrderId}", orderId);
                return StatusCode(500, new { message = "Lỗi tạo URL thanh toán", error = ex.Message });
            }
        }

        /// <summary>
        /// Callback từ VNPay sau khi user thanh toán
        /// GET: api/payment/vnpay-deposit-callback?vnp_Amount=...&vnp_ResponseCode=...
        /// </summary>
        [HttpGet("vnpay-deposit-callback")]
        public async Task<IActionResult> VNPayCallback()
        {
            try
            {
                var query = Request.Query;

                // Log toàn bộ query để debug
                _logger.LogInformation("VNPay callback: {@Query}",
                    query.ToDictionary(k => k.Key, v => v.Value.ToString()));

                // 1. Validate chữ ký từ VNPay
                var isValid = _vnpayService.ValidateCallback(query);

                if (!isValid)
                {
                    _logger.LogWarning("❌ VNPay callback - Chữ ký không hợp lệ");
                    return BadRequest(new
                    {
                        success = false,
                        message = "Chữ ký không hợp lệ"
                    });
                }

                // 2. Parse thông tin từ VNPay
                var responseCode = query["vnp_ResponseCode"].ToString();
                var txnRef = query["vnp_TxnRef"].ToString();
                var amount = query["vnp_Amount"].ToString();
                var transactionNo = query["vnp_TransactionNo"].ToString();
                var bankCode = query["vnp_BankCode"].ToString();
                var payDate = query["vnp_PayDate"].ToString();
                var orderInfo = query["vnp_OrderInfo"].ToString();

                // Parse OrderId từ TxnRef (format: {orderId}_{tick})
                var orderId = ParseOrderIdFromTxnRef(txnRef);
                if (orderId == 0)
                {
                    _logger.LogError("Không parse được OrderId từ TxnRef: {TxnRef}", txnRef);
                    return BadRequest(new { success = false, message = "TxnRef không hợp lệ" });
                }

                // Parse amount
                var actualAmount = long.TryParse(amount, out var amt) ? amt / 100 : 0;

                // 3. Lấy payment từ database
                var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
                if (payment == null)
                {
                    _logger.LogError("Không tìm thấy Payment cho Order {OrderId}", orderId);
                    return NotFound(new { success = false, message = "Không tìm thấy payment" });
                }

                // 4. Xử lý theo response code
                if (responseCode == "00")
                {
                    // Thanh toán thành công
                    var gatewayResponse = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        responseCode,
                        transactionNo,
                        bankCode,
                        payDate,
                        orderInfo,
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
                            "✅ Payment completed - Order: {OrderId}, TxnNo: {TransactionNo}, Amount: {Amount}",
                            orderId, transactionNo, actualAmount
                        );

                        // TODO: Gọi OrderService để cập nhật Order status
                        // await _orderService.ConfirmPaymentAsync(orderId);

                        return Ok(new
                        {
                            success = true,
                            message = "Thanh toán thành công",
                            data = new
                            {
                                orderId,
                                transactionNo,
                                amount = actualAmount,
                                bankCode,
                                payDate
                            }
                        });
                    }
                    else
                    {
                        _logger.LogWarning("Không thể cập nhật Payment cho Order {OrderId}", orderId);
                        return BadRequest(new { success = false, message = "Không thể cập nhật payment" });
                    }
                }
                else
                {
                    // Thanh toán thất bại
                    var errorMessage = GetVNPayErrorMessage(responseCode);

                    var gatewayResponse = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        responseCode,
                        errorMessage,
                        orderInfo
                    });

                    await _paymentService.MarkPaymentFailedAsync(orderId, gatewayResponse);

                    _logger.LogWarning(
                        "❌ Payment failed - Order: {OrderId}, Code: {ResponseCode}, Message: {Message}",
                        orderId, responseCode, errorMessage
                    );

                    return Ok(new
                    {
                        success = false,
                        message = errorMessage,
                        data = new
                        {
                            orderId,
                            responseCode
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xử lý VNPay callback");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi xử lý callback",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// IPN endpoint cho VNPay (webhook từ VNPay server)
        /// GET: api/payment/vnpay-ipn
        /// </summary>
        [HttpGet("vnpay-ipn")]
        public async Task<IActionResult> VNPayIPN()
        {
            try
            {
                var query = Request.Query;

                _logger.LogInformation("VNPay IPN received: {@Query}",
                    query.ToDictionary(k => k.Key, v => v.Value.ToString()));

                // Validate signature
                var isValid = _vnpayService.ValidateCallback(query);
                if (!isValid)
                {
                    _logger.LogWarning("VNPay IPN - Invalid signature");
                    return Ok(new { RspCode = "97", Message = "Invalid signature" });
                }

                var responseCode = query["vnp_ResponseCode"].ToString();
                var txnRef = query["vnp_TxnRef"].ToString();
                var transactionNo = query["vnp_TransactionNo"].ToString();

                var orderId = ParseOrderIdFromTxnRef(txnRef);
                if (orderId == 0)
                {
                    return Ok(new { RspCode = "01", Message = "Order not found" });
                }

                // Kiểm tra payment đã xử lý chưa (idempotency)
                var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
                if (payment == null)
                {
                    return Ok(new { RspCode = "01", Message = "Order not found" });
                }

                if (payment.IsCompleted())
                {
                    // Đã xử lý rồi, return success
                    _logger.LogInformation("VNPay IPN - Payment already processed for Order {OrderId}", orderId);
                    return Ok(new { RspCode = "00", Message = "Confirm Success" });
                }

                // Xử lý payment
                if (responseCode == "00")
                {
                    var gatewayResponse = System.Text.Json.JsonSerializer.Serialize(query.ToDictionary(k => k.Key, v => v.Value.ToString()));
                    await _paymentService.MarkPaymentCompletedAsync(orderId, transactionNo, gatewayResponse);

                    _logger.LogInformation("VNPay IPN - Payment completed for Order {OrderId}", orderId);
                }
                else
                {
                    var gatewayResponse = System.Text.Json.JsonSerializer.Serialize(new { responseCode, message = GetVNPayErrorMessage(responseCode) });
                    await _paymentService.MarkPaymentFailedAsync(orderId, gatewayResponse);

                    _logger.LogWarning("VNPay IPN - Payment failed for Order {OrderId}, Code: {ResponseCode}", orderId, responseCode);
                }

                return Ok(new { RspCode = "00", Message = "Confirm Success" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xử lý VNPay IPN");
                return Ok(new { RspCode = "99", Message = "Unknown error" });
            }
        }

        /// <summary>
        /// Query payment status
        /// GET: api/payment/status/{orderId}
        /// </summary>
        [HttpGet("status/{orderId}")]
        public async Task<IActionResult> GetPaymentStatus(int orderId)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);

                if (payment == null)
                {
                    return NotFound(new { message = $"Không tìm thấy payment cho Order #{orderId}" });
                }

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

        // ===== HELPER METHODS =====

        private int ParseOrderIdFromTxnRef(string txnRef)
        {
            try
            {
                // Format: {orderId}_{tick}
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
                "79" => "KH nhập sai mật khẩu thanh toán quá số lần quy định. Thử lại sau.",
                _ => $"Giao dịch thất bại - Mã lỗi: {responseCode}"
            };
        }
    }
}