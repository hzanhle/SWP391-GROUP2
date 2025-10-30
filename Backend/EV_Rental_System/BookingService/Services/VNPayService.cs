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
    }
}
