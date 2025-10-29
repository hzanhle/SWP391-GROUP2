namespace BookingService.DTOs
{
    public class PaymentRequirement
    {
        public decimal DepositAmount { get; set; }
        public decimal ImmediatePaymentAmount { get; set; }
        public decimal DeferredPaymentAmount { get; set; }
        public bool RequireFullPayment { get; set; }
        public string PaymentType { get; set; } // "FullPayment" hoặc "DepositOnly"
        public string Reason { get; set; }
    }
}
