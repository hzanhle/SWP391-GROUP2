namespace BookingSerivce.Models.VNPAY
{
    public class VNPaySettings
    {
        public string TmnCode { get; set; }
        public string HashSecret { get; set; }
        public string PaymentUrl { get; set; }
        public string ReturnUrl { get; set; }
        public string Version { get; set; }
        public string Command { get; set; }
        public string CurrCode { get; set; }
        public string Locale { get; set; }
    }
}
