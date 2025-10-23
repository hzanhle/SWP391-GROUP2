namespace BookingService.Services
{
    /// <summary>
    /// Interface cho PDF conversion service (có thể swap implementation)
    /// </summary>
    public interface IPdfConverterService
    {
        Task<string> ConvertHtmlToPdfAsync(string htmlContent, string outputPath);
        Task<byte[]> ConvertHtmlToPdfBytesAsync(string htmlContent);
    }
}
