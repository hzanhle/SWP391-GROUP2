using System.Globalization;

namespace BookingSerivce.Models.VNPAY
{
    /// <summary>
    /// Extension methods for easier VNPayLib usage
    /// </summary>
    public static class VNPayLibExtensions
    {
        /// <summary>
        /// Add multiple request parameters at once
        /// </summary>
        public static VNPayLib AddRequestDataBatch(
            this VNPayLib vnpay,
            Dictionary<string, string> data)
        {
            foreach (var kvp in data)
            {
                vnpay.AddRequestData(kvp.Key, kvp.Value);
            }
            return vnpay;
        }

        /// <summary>
        /// Add response data from IQueryCollection
        /// </summary>
        public static VNPayLib AddResponseDataFromQuery(
            this VNPayLib vnpay,
            IQueryCollection query)
        {
            foreach (var kvp in query)
            {
                vnpay.AddResponseData(kvp.Key, kvp.Value.ToString());
            }
            return vnpay;
        }

        /// <summary>
        /// Validate and get response code in one call
        /// </summary>
        public static (bool IsValid, string ResponseCode) ValidateAndGetResponseCode(
            this VNPayLib vnpay,
            string secureHash,
            string secretKey)
        {
            var isValid = vnpay.ValidateSignature(secureHash, secretKey);
            var responseCode = vnpay.GetResponseData("vnp_ResponseCode");

            return (isValid, responseCode);
        }
    }


}
