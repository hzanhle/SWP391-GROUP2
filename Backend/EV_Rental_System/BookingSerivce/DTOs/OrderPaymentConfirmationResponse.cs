namespace BookingSerivce.DTOs
{
    public class OrderPaymentConfirmationResponse
    {
        public bool Success { get; set; }
        public int OrderId { get; set; }
        public int PaymentId { get; set; }
        public int ContractId { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public string ContractPdfUrl { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime ConfirmedAt { get; set; }
    }
}
