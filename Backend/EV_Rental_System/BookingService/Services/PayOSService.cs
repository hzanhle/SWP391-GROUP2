using BookingService.Models.ModelSettings;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace BookingService.Services
{
    public class PayOSService : IPayOSService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly PayOSSettings _settings;
        private readonly ILogger<PayOSService> _logger;

        public PayOSService(IHttpClientFactory httpFactory, IOptions<PayOSSettings> settings, ILogger<PayOSService> logger)
        {
            _httpFactory = httpFactory;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<(bool Success, string? CheckoutUrl, string? PaymentLinkId, string? Error)> CreatePaymentLinkAsync(int orderId, decimal amount, string description)
        {
            try
            {
                using var client = _httpFactory.CreateClient();
                client.BaseAddress = new Uri(string.IsNullOrWhiteSpace(_settings.BaseUrl) ? "https://api.payos.vn" : _settings.BaseUrl.TrimEnd('/'));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
                }

                var payload = new
                {
                    orderCode = orderId.ToString(),
                    amount = amount,
                    description = description,
                    returnUrl = _settings.ReturnUrl,
                    cancelUrl = string.IsNullOrWhiteSpace(_settings.CancelUrl) ? _settings.ReturnUrl : _settings.CancelUrl,
                };

                var res = await client.PostAsJsonAsync("/v2/payment-requests", payload);
                var data = await res.Content.ReadFromJsonAsync<System.Text.Json.JsonElement?>();
                if (!res.IsSuccessStatusCode)
                {
                    var msg = data?.ToString() ?? $"HTTP {res.StatusCode}";
                    _logger.LogError("PayOS create link failed: {Message}", msg);
                    return (false, null, null, msg);
                }

                var root = data.GetValueOrDefault();
                var checkoutUrl = root.TryGetProperty("data", out var d) && d.TryGetProperty("checkoutUrl", out var u) ? u.GetString() : null;
                var linkId = d.TryGetProperty("id", out var id) ? id.GetString() : null;
                return (true, checkoutUrl, linkId, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PayOS payment link");
                return (false, null, null, ex.Message);
            }
        }

        public async Task<(bool Success, string? RefundId, string? Error)> RefundDepositAsync(string transactionId, decimal amount, string reason)
        {
            try
            {
                using var client = _httpFactory.CreateClient();
                client.BaseAddress = new Uri(string.IsNullOrWhiteSpace(_settings.BaseUrl) ? "https://api.payos.vn" : _settings.BaseUrl.TrimEnd('/'));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
                }

                var payload = new
                {
                    transactionId,
                    amount,
                    description = reason,
                };

                var res = await client.PostAsJsonAsync("/v2/refunds", payload);
                var data = await res.Content.ReadFromJsonAsync<System.Text.Json.JsonElement?>();
                if (!res.IsSuccessStatusCode)
                {
                    var msg = data?.ToString() ?? $"HTTP {res.StatusCode}";
                    _logger.LogError("PayOS refund failed: {Message}", msg);
                    return (false, null, msg);
                }

                var root = data.GetValueOrDefault();
                var refundId = root.TryGetProperty("data", out var d) && d.TryGetProperty("refundId", out var rid) ? rid.GetString() : null;
                return (true, refundId, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refunding via PayOS");
                return (false, null, ex.Message);
            }
        }

        public bool ValidateWebhookSignature(string payload, string signature)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_settings.ChecksumKey)) return false;
                using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_settings.ChecksumKey));
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload ?? string.Empty));
                var expected = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
                var given = (signature ?? string.Empty).Trim().ToLowerInvariant();
                return expected == given;
            }
            catch
            {
                return false;
            }
        }
    }
}
