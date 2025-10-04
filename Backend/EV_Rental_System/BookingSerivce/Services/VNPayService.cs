using BookingSerivce.Models.VNPAY;
using BookingService.DTOs;
using Microsoft.Extensions.Options;

namespace BookingSerivce.Services
{
    public class VNPayService : IVNPayService
    {
        private readonly VNPaySettings _settings;

        public VNPayService(IOptions<VNPaySettings> settings)
        {
            _settings = settings.Value;
        }

        public Task<string> CreatePaymentUrl(PaymentInformationModel model, string ipAddress)
        {
            var vnpay = new VNPayLib();
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var tick = DateTime.Now.Ticks.ToString();

            vnpay.AddRequestData("vnp_Version", _settings.Version);
            vnpay.AddRequestData("vnp_Command", _settings.Command);
            vnpay.AddRequestData("vnp_TmnCode", _settings.TmnCode);
            vnpay.AddRequestData("vnp_Amount", ((long)model.Amount * 100).ToString());
            vnpay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", _settings.CurrCode);
            vnpay.AddRequestData("vnp_IpAddr", ipAddress);
            vnpay.AddRequestData("vnp_Locale", _settings.Locale);
            vnpay.AddRequestData("vnp_OrderInfo", model.OrderDescription);
            vnpay.AddRequestData("vnp_OrderType", model.OrderType ?? "other");
            vnpay.AddRequestData("vnp_ReturnUrl", _settings.ReturnUrl);
            vnpay.AddRequestData("vnp_TxnRef", tick);

            var url = vnpay.CreateRequestUrl(_settings.PaymentUrl, _settings.HashSecret);
            return Task.FromResult(url);
        }


        public Task<PaymentResponseModel> ProcessPaymentResponse(Dictionary<string, string> queryParams)
        {
            var vnpay = new VNPayLib();

            foreach (var param in queryParams)
            {
                vnpay.AddResponseData(param.Key, param.Value);
            }

            var orderId = vnpay.GetResponseData("vnp_TxnRef");
            var vnpayTranId = vnpay.GetResponseData("vnp_TransactionNo");
            var responseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var transactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
            var orderInfo = vnpay.GetResponseData("vnp_OrderInfo");

            queryParams.TryGetValue("vnp_SecureHash", out var secureHash);

            var checkSignature = !string.IsNullOrEmpty(secureHash) &&
                                 vnpay.ValidateSignature(secureHash, _settings.HashSecret);

            var response = new PaymentResponseModel
            {
                Success = checkSignature && responseCode == "00" && transactionStatus == "00",
                PaymentMethod = "VNPay",
                OrderDescription = orderInfo,
                OrderId = orderId,
                TransactionId = vnpayTranId,
                Token = secureHash,
                VnPayResponseCode = responseCode
            };

            return Task.FromResult(response);
        }
    }
}
