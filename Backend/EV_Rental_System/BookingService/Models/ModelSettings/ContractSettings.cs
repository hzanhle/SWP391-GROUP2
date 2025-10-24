namespace BookingService.Models
{
    /// <summary>
    /// Configuration cho Contract service
    /// </summary>
    public class ContractSettings
    {
        public const string SectionName = "ContractSettings";

        // Storage settings
        public string StoragePath { get; set; } = "Contracts";
        public bool SaveDebugHtml { get; set; } = false;

        // Contract number format
        public string NumberPrefix { get; set; } = "CT";
        public string DateFormat { get; set; } = "yyyyMMdd";
        public int OrderIdPadding { get; set; } = 6;

        // Company default info
        public string CompanyName { get; set; } = "Công ty TNHH Cho Thuê Xe XYZ";
        public string CompanyAddress { get; set; } = "123 Đường ABC, TP. Hồ Chí Minh";
        public string CompanyTaxCode { get; set; } = "0123456789";
        public string CompanyRepresentative { get; set; } = "Ông Nguyễn Văn A";
        public string CompanyPhone { get; set; } = "1900 xxxx";
        public string CompanyEmail { get; set; } = "support@example.com";
    }
}