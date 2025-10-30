using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace BookingSerivce.Models.VNPAY
{
    /// <summary>
    /// VNPay Library for building payment URLs and validating responses
    /// </summary>
    public class VNPayLib
    {
        private readonly SortedList<string, string> _requestData;
        private readonly SortedList<string, string> _responseData;

        // Fields to exclude from signature calculation
        private static readonly HashSet<string> ExcludedSignatureFields = new()
        {
            "vnp_SecureHashType",
            "vnp_SecureHash"
        };

        public VNPayLib()
        {
            _requestData = new SortedList<string, string>(new VNPayCompare());
            _responseData = new SortedList<string, string>(new VNPayCompare());
        }

        // ===== REQUEST DATA METHODS =====

        /// <summary>
        /// Add a parameter to the payment request
        /// </summary>
        public void AddRequestData(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            if (!string.IsNullOrEmpty(value))
            {
                _requestData[key] = value; // Use indexer to allow update
            }
        }

        /// <summary>
        /// Get request data as query string (for signature calculation)
        /// </summary>
        public string GetRequestDataString()
        {
            return BuildQueryString(_requestData, excludeSignatureFields: false);
        }

        /// <summary>
        /// Create payment URL with signature
        /// </summary>
        public string CreateRequestUrl(string baseUrl, string hashSecret)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentException("Base URL cannot be null or empty", nameof(baseUrl));

            if (string.IsNullOrWhiteSpace(hashSecret))
                throw new ArgumentException("Hash secret cannot be null or empty", nameof(hashSecret));

            // Build query string
            var queryString = BuildQueryString(_requestData, excludeSignatureFields: false);

            // Calculate signature
            var signature = HmacSHA512(hashSecret, queryString);

            // Build final URL
            var separator = baseUrl.Contains('?') ? "&" : "?";
            return $"{baseUrl}{separator}{queryString}&vnp_SecureHash={signature}";
        }

        // ===== RESPONSE DATA METHODS =====

        /// <summary>
        /// Add a parameter from VNPay callback/IPN response
        /// </summary>
        public void AddResponseData(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            if (!string.IsNullOrEmpty(value))
            {
                _responseData[key] = value;
            }
        }

        /// <summary>
        /// Get a specific response parameter value
        /// </summary>
        public string GetResponseData(string key)
        {
            return _responseData.TryGetValue(key, out var value) ? value : string.Empty;
        }

        /// <summary>
        /// Get response data as query string (excluding signature fields)
        /// </summary>
        public string GetResponseDataString()
        {
            return BuildQueryString(_responseData, excludeSignatureFields: true);
        }

        /// <summary>
        /// Validate signature from VNPay callback/IPN
        /// </summary>
        public bool ValidateSignature(string inputHash, string secretKey)
        {
            if (string.IsNullOrWhiteSpace(inputHash))
                return false;

            if (string.IsNullOrWhiteSpace(secretKey))
                throw new ArgumentException("Secret key cannot be null or empty", nameof(secretKey));

            // Get query string without signature fields
            var dataToSign = GetResponseDataString();

            // Calculate expected signature
            var expectedSignature = HmacSHA512(secretKey, dataToSign);

            // Compare (case-insensitive)
            return string.Equals(expectedSignature, inputHash, StringComparison.OrdinalIgnoreCase);
        }

        // ===== HELPER METHODS =====

        /// <summary>
        /// Build URL-encoded query string from sorted dictionary
        /// </summary>
        private static string BuildQueryString(
            SortedList<string, string> data,
            bool excludeSignatureFields)
        {
            var builder = new StringBuilder();

            foreach (var kvp in data)
            {
                // Skip empty values
                if (string.IsNullOrEmpty(kvp.Value))
                    continue;

                // Skip signature fields if requested
                if (excludeSignatureFields && ExcludedSignatureFields.Contains(kvp.Key))
                    continue;

                if (builder.Length > 0)
                    builder.Append('&');

                builder.Append(WebUtility.UrlEncode(kvp.Key));
                builder.Append('=');
                builder.Append(WebUtility.UrlEncode(kvp.Value));
            }

            return builder.ToString();
        }

        /// <summary>
        /// Calculate HMAC-SHA512 signature
        /// </summary>
        public static string HmacSHA512(string key, string inputData)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            if (string.IsNullOrEmpty(inputData))
                throw new ArgumentException("Input data cannot be null or empty", nameof(inputData));

            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);

            using var hmac = new HMACSHA512(keyBytes);
            var hashBytes = hmac.ComputeHash(inputBytes);

            // Convert to hex string
            return Convert.ToHexString(hashBytes).ToLower();
        }

        // ===== UTILITY METHODS =====

        /// <summary>
        /// Clear all request data
        /// </summary>
        public void ClearRequestData()
        {
            _requestData.Clear();
        }

        /// <summary>
        /// Clear all response data
        /// </summary>
        public void ClearResponseData()
        {
            _responseData.Clear();
        }

        /// <summary>
        /// Get all request data as dictionary (for debugging)
        /// </summary>
        public IReadOnlyDictionary<string, string> GetAllRequestData()
        {
            return new Dictionary<string, string>(_requestData);
        }

        /// <summary>
        /// Get all response data as dictionary (for debugging)
        /// </summary>
        public IReadOnlyDictionary<string, string> GetAllResponseData()
        {
            return new Dictionary<string, string>(_responseData);
        }
    }
}

