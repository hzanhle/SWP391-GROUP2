using BookingService.Models.ModelSettings;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

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

        // === HTTP client (ưu tiên IPv4 nếu cần) ===
        private HttpClient CreateHttpClient()
        {
            HttpMessageHandler handler;

            if (_settings.PreferIPv4)
            {
                handler = new SocketsHttpHandler
                {
                    ConnectCallback = async (ctx, ct) =>
                    {
                        var host = ctx.DnsEndPoint.Host;
                        var port = ctx.DnsEndPoint.Port;
                        var addrs = await Dns.GetHostAddressesAsync(host).ConfigureAwait(false);
                        var ipv4 = addrs.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)
                                   ?? addrs.FirstOrDefault();
                        if (ipv4 == null) throw new SocketException((int)SocketError.HostNotFound);

                        var s = new Socket(ipv4.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                        await s.ConnectAsync(new IPEndPoint(ipv4, port), ct).ConfigureAwait(false);
                        return new NetworkStream(s, ownsSocket: true);
                    }
                };
            }
            else
            {
                handler = new SocketsHttpHandler();
            }

            var baseUrl = string.IsNullOrWhiteSpace(_settings.BaseUrl)
                ? "https://api-merchant.payos.vn"
                : _settings.BaseUrl.TrimEnd('/');

            var client = new HttpClient(handler, disposeHandler: true)
            {
                DefaultRequestVersion = HttpVersion.Version11,
                BaseAddress = new Uri(baseUrl)
            };

            // Headers bắt buộc của PayOS
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Remove("x-client-id");
            client.DefaultRequestHeaders.Remove("x-api-key");
            client.DefaultRequestHeaders.Add("x-client-id", _settings.ClientId ?? "");
            client.DefaultRequestHeaders.Add("x-api-key", _settings.ApiKey ?? "");

            // Không dùng Bearer cho ApiKey của PayOS
            client.DefaultRequestHeaders.Authorization = null;

            return client;
        }

        // === Tạo chữ ký HMAC-SHA256 cho payment-requests ===
        // Ký theo chuỗi: amount=&cancelUrl=&description=&orderCode=&returnUrl=
        private static string BuildCreateSignature(long orderCode, int amount, string description, string returnUrl, string cancelUrl, string secret)
        {
            var parts = new[]
            {
                $"amount={amount}",
                $"cancelUrl={cancelUrl ?? ""}",
                $"description={description ?? ""}",
                $"orderCode={orderCode}",
                $"returnUrl={returnUrl ?? ""}"
            };
            var data = string.Join("&", parts);

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret ?? ""));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        public async Task<(bool Success, string? CheckoutUrl, string? PaymentLinkId, string? Error)>
            CreatePaymentLinkAsync(int orderId, decimal amount, string description)
        {
            try
            {
                using var client = CreateHttpClient();

                // PayOS yêu cầu số nguyên
                long orderCode = orderId; // hoặc ID đơn hàng thực tế/unique code
                int amountVnd = (int)Math.Round(amount, MidpointRounding.AwayFromZero);

                // Mô tả ngắn gọn để an toàn (một số kênh có giới hạn độ dài)
                var desc = string.IsNullOrWhiteSpace(description) ? "DEP" : description.Trim();
                if (desc.Length > 64) desc = desc.Substring(0, 64); // tuỳ bạn muốn cắt bao nhiêu

                var returnUrl = _settings.ReturnUrl;
                var cancelUrl = string.IsNullOrWhiteSpace(_settings.CancelUrl) ? _settings.ReturnUrl : _settings.CancelUrl;

                // signature bắt buộc (nếu PayOS bật kiểm tra)
                var signature = BuildCreateSignature(orderCode, amountVnd, desc, returnUrl, cancelUrl, _settings.ChecksumKey ?? "");

                var payload = new
                {
                    orderCode = orderCode,
                    amount = amountVnd,
                    description = desc,
                    returnUrl,
                    cancelUrl,
                    signature
                };

                var (res, text, endpointUsed) = await PostWithFallbackAsync(
                    client,
                    payload,
                    new[] { "api/v2/payment-requests", "v2/payment-requests" },
                    nameof(CreatePaymentLinkAsync));

                _logger.LogDebug("PayOS create endpoint used: {Endpoint}", endpointUsed);

                _logger.LogInformation("PayOS create response (HTTP {Status}): {Body}", (int)res.StatusCode, text);

                if (!res.IsSuccessStatusCode)
                {
                    return (false, null, null, text);
                }

                using var doc = JsonDocument.Parse(text);
                var root = doc.RootElement;

                var code = root.TryGetProperty("code", out var codeEl) && codeEl.ValueKind == JsonValueKind.String
                    ? codeEl.GetString()
                    : null;
                var descMsg = root.TryGetProperty("desc", out var descEl) && descEl.ValueKind == JsonValueKind.String
                    ? descEl.GetString()
                    : null;

                if (!string.Equals(code, "00", StringComparison.OrdinalIgnoreCase))
                {
                    return (false, null, null, $"{code ?? "??"}: {descMsg ?? "Unknown PayOS error"}");
                }

                if (root.TryGetProperty("data", out var dataEl) && dataEl.ValueKind == JsonValueKind.Object)
                {
                    string? checkoutUrl = null, linkId = null;
                    if (dataEl.TryGetProperty("checkoutUrl", out var u) && u.ValueKind == JsonValueKind.String)
                        checkoutUrl = u.GetString();
                    if (dataEl.TryGetProperty("id", out var idEl) && idEl.ValueKind == JsonValueKind.String)
                        linkId = idEl.GetString();

                    return (true, checkoutUrl, linkId, null);
                }

                return (false, null, null, "PayOS: data is null or invalid.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PayOS payment link");
                return (false, null, null, ex.Message);
            }
        }

        public async Task<(bool Success, string? RefundId, string? Error)>
            RefundDepositAsync(string transactionId, decimal amount, string reason)
        {
            try
            {
                using var client = CreateHttpClient();

                int amountVnd = (int)Math.Round(amount, MidpointRounding.AwayFromZero);
                var payload = new
                {
                    transactionId,
                    amount = amountVnd,
                    description = reason
                };

                var (res, text, endpointUsed) = await PostWithFallbackAsync(
                    client,
                    payload,
                    new[] { "api/v2/refunds", "v2/refunds" },
                    nameof(RefundDepositAsync));

                _logger.LogDebug("PayOS refund endpoint used: {Endpoint}", endpointUsed);

                _logger.LogInformation("PayOS refund response (HTTP {Status}): {Body}", (int)res.StatusCode, text);

                if (!res.IsSuccessStatusCode)
                {
                    return (false, null, text);
                }

                using var doc = JsonDocument.Parse(text);
                var root = doc.RootElement;

                var code = root.TryGetProperty("code", out var codeEl) && codeEl.ValueKind == JsonValueKind.String
                    ? codeEl.GetString()
                    : null;
                var descMsg = root.TryGetProperty("desc", out var descEl) && descEl.ValueKind == JsonValueKind.String
                    ? descEl.GetString()
                    : null;

                if (!string.Equals(code, "00", StringComparison.OrdinalIgnoreCase))
                {
                    return (false, null, $"{code ?? "??"}: {descMsg ?? "Unknown PayOS error"}");
                }

                string? refundId = null;
                if (root.TryGetProperty("data", out var dataEl) && dataEl.ValueKind == JsonValueKind.Object &&
                    dataEl.TryGetProperty("refundId", out var rid) && rid.ValueKind == JsonValueKind.String)
                {
                    refundId = rid.GetString();
                }

                return (true, refundId, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refunding via PayOS");
                return (false, null, ex.Message);
            }
        }

        // Xác minh webhook (nếu PayOS gửi signature)
        public bool ValidateWebhookSignature(string payload, string signature)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_settings.ChecksumKey)) return false;
                using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_settings.ChecksumKey));
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload ?? string.Empty));
                var expected = Convert.ToHexString(hash).ToLowerInvariant();
                var given = (signature ?? string.Empty).Trim().ToLowerInvariant();
                return expected == given;
            }
            catch
            {
                return false;
            }
        }
        private async Task<(HttpResponseMessage Response, string Body, string Endpoint)> PostWithFallbackAsync(
            HttpClient client,
            object payload,
            string[] endpoints,
            string callerName)
        {
            HttpResponseMessage? response = null;
            string? body = null;
            string endpointUsed = endpoints.Last();

            foreach (var endpoint in endpoints)
            {
                response = await client.PostAsJsonAsync(endpoint, payload);
                body = await response.Content.ReadAsStringAsync();
                endpointUsed = endpoint;

                if (response.StatusCode != HttpStatusCode.NotFound)
                {
                    break;
                }

                _logger.LogWarning(
                    "PayOS endpoint {Endpoint} returned 404 for {Caller}. Trying next fallback if available.",
                    endpoint,
                    callerName);
            }

            return (response!, body ?? string.Empty, endpointUsed);
        }
    }
}
