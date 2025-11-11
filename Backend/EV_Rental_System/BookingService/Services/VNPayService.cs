using BookingSerivce.DTOs;
using BookingSerivce.Models.VNPAY;
using Microsoft.Extensions.Options;

namespace BookingService.Services
{
    public class VNPayService : IVNPayService
    {
        private readonly VNPaySettings _settings;
        private readonly HttpClient _httpClient;
        private readonly ILogger<VNPayService> _logger;

        public VNPayService(
            IOptions<VNPaySettings> options,
            HttpClient httpClient,
            ILogger<VNPayService> logger)
        {
            _settings = options.Value;
            _httpClient = httpClient;
            _logger = logger;
        }

        // ===== Existing Methods =====

        public string CreatePaymentUrl(int orderId, decimal amount, string description)
        {
            var tick = DateTime.Now.Ticks.ToString();
            var vnp = new VNPayLib();

            vnp.AddRequestData("vnp_Version", _settings.Version);
            vnp.AddRequestData("vnp_Command", _settings.Command);
            vnp.AddRequestData("vnp_TmnCode", _settings.TmnCode);
            vnp.AddRequestData("vnp_Amount", ((int)amount * 100).ToString());
            vnp.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnp.AddRequestData("vnp_CurrCode", _settings.CurrCode);
            vnp.AddRequestData("vnp_IpAddr", "127.0.0.1");
            vnp.AddRequestData("vnp_Locale", _settings.Locale);
            vnp.AddRequestData("vnp_OrderInfo", description);
            vnp.AddRequestData("vnp_OrderType", "other");
            vnp.AddRequestData("vnp_ReturnUrl", _settings.ReturnUrl);
            vnp.AddRequestData("vnp_TxnRef", $"{orderId}_{tick}");

            return vnp.CreateRequestUrl(_settings.PaymentUrl, _settings.HashSecret);
        }

        public string CreateAdditionalPaymentUrl(int settlementId, int orderId, decimal amount, string description)
        {
            // Validate amount
            if (amount <= 0)
            {
                throw new ArgumentException($"Payment amount must be greater than 0. Received: {amount:F2} VND", nameof(amount));
            }

            // VNPay typically requires minimum 1,000 VND (10,000 smallest units)
            if (amount < 1000)
            {
                throw new ArgumentException(
                    $"Payment amount must be at least 1,000 VND for VNPay transactions. Received: {amount:F2} VND",
                    nameof(amount));
            }

            var tick = DateTime.Now.Ticks.ToString();
            var vnp = new VNPayLib();

            vnp.AddRequestData("vnp_Version", _settings.Version);
            vnp.AddRequestData("vnp_Command", _settings.Command);
            vnp.AddRequestData("vnp_TmnCode", _settings.TmnCode);

            // Convert to VNPay amount format (smallest currency unit - multiply by 100)
            // Use long to avoid integer overflow for large amounts
            long vnpayAmount = (long)(amount * 100);
            vnp.AddRequestData("vnp_Amount", vnpayAmount.ToString());
            vnp.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnp.AddRequestData("vnp_CurrCode", _settings.CurrCode);
            vnp.AddRequestData("vnp_IpAddr", "127.0.0.1");
            vnp.AddRequestData("vnp_Locale", _settings.Locale);
            vnp.AddRequestData("vnp_OrderInfo", description);
            vnp.AddRequestData("vnp_OrderType", "other");

            // Different return URL for settlement payments
            var settlementReturnUrl = _settings.ReturnUrl.Replace("/payment/vnpay-deposit-callback", "/settlement/payment-return");
            vnp.AddRequestData("vnp_ReturnUrl", settlementReturnUrl);

            // TxnRef format: SETTLEMENT_{settlementId}_{orderId}_{tick}
            vnp.AddRequestData("vnp_TxnRef", $"SETTLEMENT_{settlementId}_{orderId}_{tick}");

            return vnp.CreateRequestUrl(_settings.PaymentUrl, _settings.HashSecret);
        }

        public bool ValidateCallback(IQueryCollection query)
        {
            var vnp = new VNPayLib();
            foreach (var key in query.Keys)
            {
                vnp.AddResponseData(key, query[key]);
            }

            var vnpSecureHash = query["vnp_SecureHash"];
            return vnp.ValidateSignature(vnpSecureHash, _settings.HashSecret);
        }

        // ===== NEW: Query Transaction Status =====

        /// <summary>
        /// Query transaction status from VNPay using QueryDR API
        /// </summary>
        /// <param name="txnRef">Transaction reference (OrderId_Tick)</param>
        /// <param name="transactionDate">Date when transaction was created</param>
        /// <returns>VNPayQueryResponse or null if error</returns>
        public async Task<VNPayQueryResponse?> QueryTransactionAsync(string txnRef, DateTime transactionDate)
        {
            try
            {
                _logger.LogInformation("Querying VNPay transaction: {TxnRef}", txnRef);

                // Build request
                var vnp = new VNPayLib();
                var requestId = DateTime.Now.Ticks.ToString();
                var createDate = DateTime.Now.ToString("yyyyMMddHHmmss");
                var transDate = transactionDate.ToString("yyyyMMdd");

                // Add query parameters (theo thứ tự alphabet)
                vnp.AddRequestData("vnp_Command", "querydr");
                vnp.AddRequestData("vnp_CreateDate", createDate);
                vnp.AddRequestData("vnp_IpAddr", "127.0.0.1");
                vnp.AddRequestData("vnp_OrderInfo", $"Query transaction {txnRef}");
                vnp.AddRequestData("vnp_RequestId", requestId);
                vnp.AddRequestData("vnp_TmnCode", _settings.TmnCode);
                vnp.AddRequestData("vnp_TransactionDate", transDate);
                vnp.AddRequestData("vnp_TxnRef", txnRef);
                vnp.AddRequestData("vnp_Version", _settings.Version);

                // Create secure hash
                var signData = vnp.GetRequestDataString();
                var secureHash = VNPayLib.HmacSHA512(_settings.HashSecret, signData);
                vnp.AddRequestData("vnp_SecureHash", secureHash);

                // Build query URL
                var queryUrl = vnp.CreateRequestUrl(_settings.QueryUrl, _settings.HashSecret);

                _logger.LogDebug("VNPay QueryDR URL: {Url}", queryUrl);

                // Send GET request
                var response = await _httpClient.GetAsync(queryUrl);
                var content = await response.Content.ReadAsStringAsync();

                _logger.LogDebug("VNPay QueryDR Response: {Content}", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("VNPay QueryDR failed with status {Status}", response.StatusCode);
                    return null;
                }

                // Parse response
                var queryParams = System.Web.HttpUtility.ParseQueryString(content);
                var result = new VNPayQueryResponse
                {
                    ResponseCode = queryParams["vnp_ResponseCode"] ?? "",
                    Message = GetQueryResponseMessage(queryParams["vnp_ResponseCode"] ?? ""),
                    TransactionNo = queryParams["vnp_TransactionNo"] ?? "",
                    TransactionStatus = queryParams["vnp_TransactionStatus"] ?? "",
                    Amount = decimal.TryParse(queryParams["vnp_Amount"], out var amt) ? amt / 100 : 0,
                    BankCode = queryParams["vnp_BankCode"] ?? "",
                    OrderInfo = queryParams["vnp_OrderInfo"] ?? ""
                };

                // Parse PayDate if exists
                if (DateTime.TryParseExact(
                    queryParams["vnp_PayDate"],
                    "yyyyMMddHHmmss",
                    null,
                    System.Globalization.DateTimeStyles.None,
                    out var payDate))
                {
                    result.PayDate = payDate;
                }

                _logger.LogInformation(
                    "VNPay Query Result - TxnRef: {TxnRef}, RspCode: {Code}, TxnStatus: {Status}, TransNo: {TransNo}",
                    txnRef, result.ResponseCode, result.TransactionStatus, result.TransactionNo
                );

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying VNPay transaction {TxnRef}", txnRef);
                return null;
            }
        }

        // ===== NEW: Process Refund =====

        /// <summary>
        /// Process refund through VNPay Refund API
        /// </summary>
        /// <param name="txnRef">Original transaction reference (OrderId_Tick)</param>
        /// <param name="amount">Amount to refund (VND)</param>
        /// <param name="orderInfo">Refund description</param>
        /// <param name="transactionNo">Original VNPay transaction number</param>
        /// <param name="transactionDate">Date when original transaction was paid</param>
        /// <param name="createdBy">User ID who initiated refund</param>
        /// <returns>VNPayRefundResponse or null if error</returns>
        public async Task<VNPayRefundResponse?> ProcessRefundAsync(
            string txnRef,
            decimal amount,
            string orderInfo,
            string transactionNo,
            DateTime transactionDate,
            int createdBy)
        {
            try
            {
                _logger.LogInformation("Processing VNPay refund: TxnRef={TxnRef}, Amount={Amount}", txnRef, amount);

                // Build request
                var vnp = new VNPayLib();
                var requestId = DateTime.Now.Ticks.ToString();
                var createDate = DateTime.Now.ToString("yyyyMMddHHmmss");
                var transDate = transactionDate.ToString("yyyyMMddHHmmss");

                // Add refund parameters (theo thứ tự alphabet)
                vnp.AddRequestData("vnp_Amount", ((int)(amount * 100)).ToString()); // Convert to xu
                vnp.AddRequestData("vnp_Command", "refund");
                vnp.AddRequestData("vnp_CreateBy", createdBy.ToString());
                vnp.AddRequestData("vnp_CreateDate", createDate);
                vnp.AddRequestData("vnp_IpAddr", "127.0.0.1");
                vnp.AddRequestData("vnp_OrderInfo", orderInfo);
                vnp.AddRequestData("vnp_RequestId", requestId);
                vnp.AddRequestData("vnp_TmnCode", _settings.TmnCode);
                vnp.AddRequestData("vnp_TransactionDate", transDate);
                vnp.AddRequestData("vnp_TransactionNo", transactionNo);
                vnp.AddRequestData("vnp_TransactionType", "02"); // 02 = full refund, 03 = partial refund
                vnp.AddRequestData("vnp_TxnRef", txnRef);
                vnp.AddRequestData("vnp_Version", _settings.Version);

                // Create secure hash
                var signData = vnp.GetRequestDataString();
                var secureHash = VNPayLib.HmacSHA512(_settings.HashSecret, signData);
                vnp.AddRequestData("vnp_SecureHash", secureHash);

                // Build refund URL
                var refundUrl = vnp.CreateRequestUrl(_settings.RefundUrl, _settings.HashSecret);

                _logger.LogDebug("VNPay Refund URL: {Url}", refundUrl);

                // Send GET request
                var response = await _httpClient.GetAsync(refundUrl);
                var content = await response.Content.ReadAsStringAsync();

                _logger.LogDebug("VNPay Refund Response: {Content}", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("VNPay Refund failed with status {Status}", response.StatusCode);
                    return null;
                }

                // Parse response
                var queryParams = System.Web.HttpUtility.ParseQueryString(content);
                var result = new VNPayRefundResponse
                {
                    ResponseCode = queryParams["vnp_ResponseCode"] ?? "",
                    Message = GetRefundResponseMessage(queryParams["vnp_ResponseCode"] ?? ""),
                    TransactionNo = queryParams["vnp_TransactionNo"] ?? "",
                    TxnRef = queryParams["vnp_TxnRef"] ?? txnRef,
                    Amount = decimal.TryParse(queryParams["vnp_Amount"], out var amt) ? amt / 100 : 0,
                    BankCode = queryParams["vnp_BankCode"] ?? "",
                    OrderInfo = queryParams["vnp_OrderInfo"] ?? "",
                    RawResponse = content
                };

                // Parse RefundDate if exists
                if (DateTime.TryParseExact(
                    queryParams["vnp_PayDate"],
                    "yyyyMMddHHmmss",
                    null,
                    System.Globalization.DateTimeStyles.None,
                    out var refundDate))
                {
                    result.RefundDate = refundDate;
                }

                _logger.LogInformation(
                    "VNPay Refund Result - TxnRef: {TxnRef}, RspCode: {Code}, TransNo: {TransNo}",
                    txnRef, result.ResponseCode, result.TransactionNo
                );

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing VNPay refund for {TxnRef}", txnRef);
                return null;
            }
        }

        // ===== Helper Methods =====

        private string GetQueryResponseMessage(string responseCode)
        {
            return responseCode switch
            {
                "00" => "Giao dịch thành công",
                "91" => "Không tìm thấy giao dịch",
                "94" => "Yêu cầu bị trùng lặp",
                "97" => "Chữ ký không hợp lệ",
                "99" => "Lỗi không xác định",
                _ => $"Unknown response code: {responseCode}"
            };
        }

        private string GetRefundResponseMessage(string responseCode)
        {
            return responseCode switch
            {
                "00" => "Hoàn tiền thành công",
                "02" => "Số dư tài khoản không đủ để hoàn tiền",
                "91" => "Không tìm thấy giao dịch gốc",
                "93" => "Giao dịch đã được hoàn trước đó",
                "94" => "Yêu cầu bị trùng lặp",
                "95" => "Giao dịch chưa được thanh toán",
                "97" => "Chữ ký không hợp lệ",
                "98" => "Timeout - vui lòng query lại",
                "99" => "Lỗi không xác định",
                _ => $"Unknown response code: {responseCode}"
            };
        }
    }
}
