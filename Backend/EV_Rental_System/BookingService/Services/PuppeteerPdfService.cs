using BookingService.Models.ModelSettings;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using PuppeteerSharp.Media;
namespace BookingService.Services
{
    /// <summary>
    /// PDF Converter sử dụng PuppeteerSharp (Chromium headless)
    /// </summary>
    public class PuppeteerPdfService : IPdfConverterService, IDisposable
    {
        private readonly ILogger<PuppeteerPdfService> _logger;
        private readonly PdfSettings _settings;
        private IBrowser? _browser;
        private readonly SemaphoreSlim _browserLock = new(1, 1);
        private bool _isChromiumDownloaded = false;

        public PuppeteerPdfService(
            ILogger<PuppeteerPdfService> logger,
            IOptions<PdfSettings> settings)
        {
            _logger = logger;
            _settings = settings.Value;
        }

        /// <summary>
        /// Convert HTML to PDF file
        /// </summary>
        public async Task<string> ConvertHtmlToPdfAsync(string htmlContent, string outputPath)
        {
            try
            {
                _logger.LogInformation("Converting HTML to PDF: {OutputPath}", outputPath);

                // Ensure browser is ready
                await EnsureBrowserAsync();

                // Create new page
                var page = await _browser!.NewPageAsync();

                try
                {
                    // Set content with timeout
                    await page.SetContentAsync(htmlContent, new NavigationOptions
                    {
                        WaitUntil = new[] { WaitUntilNavigation.Networkidle0 },
                        Timeout = _settings.PageLoadTimeoutMs
                    });

                    // Generate PDF
                    await page.PdfAsync(outputPath, CreatePdfOptions());

                    _logger.LogInformation("PDF generated successfully: {OutputPath}", outputPath);
                    return outputPath;
                }
                finally
                {
                    await page.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting HTML to PDF");
                throw new InvalidOperationException($"Không thể tạo PDF: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Convert HTML to PDF bytes (không lưu file)
        /// </summary>
        public async Task<byte[]> ConvertHtmlToPdfBytesAsync(string htmlContent)
        {
            try
            {
                await EnsureBrowserAsync();
                var page = await _browser!.NewPageAsync();

                try
                {
                    await page.SetContentAsync(htmlContent, new NavigationOptions
                    {
                        WaitUntil = new[] { WaitUntilNavigation.Networkidle0 },
                        Timeout = _settings.PageLoadTimeoutMs
                    });

                    var pdfStream = await page.PdfStreamAsync(CreatePdfOptions());
                    using var memoryStream = new MemoryStream();
                    await pdfStream.CopyToAsync(memoryStream);

                    return memoryStream.ToArray();
                }
                finally
                {
                    await page.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting HTML to PDF bytes");
                throw new InvalidOperationException($"Không thể tạo PDF: {ex.Message}", ex);
            }
        }

        #region Private Methods

        /// <summary>
        /// Ensure Chromium is downloaded and browser is launched
        /// </summary>
        private async Task EnsureBrowserAsync()
        {
            await _browserLock.WaitAsync();
            try
            {
                // Download Chromium if needed (chỉ 1 lần)
                if (!_isChromiumDownloaded)
                {
                    _logger.LogInformation("Downloading Chromium browser...");
                    var browserFetcher = new BrowserFetcher();
                    await browserFetcher.DownloadAsync();
                    _isChromiumDownloaded = true;
                    _logger.LogInformation("Chromium downloaded successfully");
                }

                // Launch browser if not already running
                if (_browser == null || _browser.IsClosed)
                {
                    _logger.LogInformation("Launching browser...");
                    _browser = await Puppeteer.LaunchAsync(new LaunchOptions
                    {
                        Headless = true,
                        Args = new[]
                        {
                            "--no-sandbox",
                            "--disable-setuid-sandbox",
                            "--disable-dev-shm-usage",
                            "--disable-gpu",
                            "--disable-software-rasterizer",
                            "--disable-extensions"
                        },
                        Timeout = _settings.BrowserLaunchTimeoutMs
                    });
                    _logger.LogInformation("Browser launched successfully");
                }
            }
            finally
            {
                _browserLock.Release();
            }
        }

        /// <summary>
        /// Create PDF options from settings
        /// </summary>
        private PdfOptions CreatePdfOptions()
        {
            return new PdfOptions
            {
                Format = ParsePaperFormat(_settings.PageFormat),
                PrintBackground = _settings.PrintBackground,
                PreferCSSPageSize = false,
                DisplayHeaderFooter = _settings.DisplayHeaderFooter,
                HeaderTemplate = _settings.HeaderTemplate,
                FooterTemplate = _settings.FooterTemplate,
                MarginOptions = new MarginOptions
                {
                    Top = _settings.MarginTop,
                    Bottom = _settings.MarginBottom,
                    Left = _settings.MarginLeft,
                    Right = _settings.MarginRight
                },
                Scale = _settings.Scale
            };
        }

        /// <summary>
        /// Parse paper format string
        /// </summary>
        private PaperFormat ParsePaperFormat(string format)
        {
            return format.ToUpper() switch
            {
                "A4" => PaperFormat.A4,
                "A3" => PaperFormat.A3,
                "LETTER" => PaperFormat.Letter,
                "LEGAL" => PaperFormat.Legal,
                _ => PaperFormat.A4
            };
        }

        #endregion

        public void Dispose()
        {
            _browser?.Dispose();
            _browserLock?.Dispose();
        }
    }
}