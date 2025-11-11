namespace BookingService.Models.ModelSettings
{
    public class PayOSSettings
    {
        public string BaseUrl { get; set; } = "https://api.payos.vn";
        public string ClientId { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ChecksumKey { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public string CancelUrl { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
    }
}
