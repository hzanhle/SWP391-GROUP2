namespace BookingSerivce.Models.VNPAY
{
    public class VNPaySettings
    {
        public string TmnCode { get; set; } = string.Empty;
        public string HashSecret { get; set; } = string.Empty;
        public string PaymentUrl { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public string QueryUrl { get; set; } = string.Empty; // For QueryDR API
        public string RefundUrl { get; set; } = string.Empty; // For Refund API
        public string Version { get; set; } = "2.1.0";
        public string Command { get; set; } = "pay";
        public string CurrCode { get; set; } = "VND";
        public string Locale { get; set; } = "vn";
    }
}
