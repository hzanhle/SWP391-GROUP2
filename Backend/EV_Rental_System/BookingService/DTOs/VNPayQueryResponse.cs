namespace BookingSerivce.DTOs
{
    public class VNPayQueryResponse
    {
        public string ResponseCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string TransactionNo { get; set; } = string.Empty;
        public string TransactionStatus { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string BankCode { get; set; } = string.Empty;
        public string OrderInfo { get; set; } = string.Empty;
        public DateTime? PayDate { get; set; }

        // Helper methods
        public bool IsSuccess => ResponseCode == "00";
        public bool IsTransactionSuccess => TransactionStatus == "00";
        public bool IsTransactionNotFound => ResponseCode == "91";
    }
}
