using BookingSerivce.Models.VNPAY;
using Microsoft.Extensions.Options;

namespace BookingService.Services
{
    public class VNPayService : IVNPayService
    {
        private readonly VNPaySettings _settings;

        public VNPayService(IOptions<VNPaySettings> options)
        {
            _settings = options.Value;
        }

        public string CreatePaymentUrl(int orderId, decimal amount, string description)
        {
            var tick = DateTime.Now.Ticks.ToString();
            var vnp = new VNPayLib();

            vnp.AddRequestData("vnp_Version", _settings.Version);
            vnp.AddRequestData("vnp_Command", _settings.Command);
            vnp.AddRequestData("vnp_TmnCode", _settings.TmnCode);
            vnp.AddRequestData("vnp_Amount", ((int)amount * 100).ToString()); // VNPay yêu cầu *100
            vnp.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnp.AddRequestData("vnp_CurrCode", _settings.CurrCode);
            vnp.AddRequestData("vnp_IpAddr", "127.0.0.1"); // hoặc HttpContext.Connection.RemoteIpAddress
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
    }
}
