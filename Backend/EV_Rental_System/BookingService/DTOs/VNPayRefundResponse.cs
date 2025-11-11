namespace BookingSerivce.DTOs
{
    /// <summary>
    /// Response from VNPay Refund API
    /// </summary>
    public class VNPayRefundResponse
    {
        /// <summary>
        /// Response code from VNPay (00 = success)
        /// </summary>
        public string ResponseCode { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// VNPay transaction number for the refund
        /// </summary>
        public string TransactionNo { get; set; } = string.Empty;

        /// <summary>
        /// Original transaction reference (OrderId_Tick)
        /// </summary>
        public string TxnRef { get; set; } = string.Empty;

        /// <summary>
        /// Refund amount (VND)
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Bank code
        /// </summary>
        public string BankCode { get; set; } = string.Empty;

        /// <summary>
        /// Order info / description
        /// </summary>
        public string OrderInfo { get; set; } = string.Empty;

        /// <summary>
        /// Date when refund was processed
        /// </summary>
        public DateTime? RefundDate { get; set; }

        /// <summary>
        /// Full response data from VNPay (for logging/debugging)
        /// </summary>
        public string? RawResponse { get; set; }

        // Helper methods
        public bool IsSuccess => ResponseCode == "00";
        public bool IsInvalidSignature => ResponseCode == "97";
        public bool IsInsufficientBalance => ResponseCode == "02";
    }
}
