namespace BookingService.Models
{
    /// <summary>
    /// Configuration settings cho PDF generation
    /// </summary>
    public class PdfSettings
    {
        public const string SectionName = "PdfSettings";

        // Browser settings
        public int BrowserLaunchTimeoutMs { get; set; } = 30000; // 30 seconds
        public int PageLoadTimeoutMs { get; set; } = 30000; // 30 seconds

        // PDF format settings
        public string PageFormat { get; set; } = "A4"; // A4, A3, Letter, Legal
        public bool PrintBackground { get; set; } = true;
        public bool DisplayHeaderFooter { get; set; } = false;
        public decimal Scale { get; set; } = 1.0m;

        // Margin settings (CSS units: px, cm, mm, in)
        public string MarginTop { get; set; } = "40px";
        public string MarginBottom { get; set; } = "40px";
        public string MarginLeft { get; set; } = "40px";
        public string MarginRight { get; set; } = "40px";

        // Header/Footer templates (HTML)
        public string? HeaderTemplate { get; set; }
        public string? FooterTemplate { get; set; }

        // Performance settings
        public bool ReuseChromiumInstance { get; set; } = true;
        public int MaxConcurrentConversions { get; set; } = 3;
    }
}

