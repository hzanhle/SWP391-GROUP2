namespace BookingService.DTOs
{
    /// <summary>
    /// Response containing settlement details
    /// </summary>
    public class SettlementResponse
    {
        public int SettlementId { get; set; }
        public int OrderId { get; set; }

        // Timing
        public DateTime ScheduledReturnTime { get; set; }
        public DateTime ActualReturnTime { get; set; }
        public bool IsLate => ActualReturnTime > ScheduledReturnTime;

        // Overtime
        public decimal OvertimeHours { get; set; }
        public decimal OvertimeFee { get; set; }

        // Damage
        public decimal DamageCharge { get; set; }
        public string? DamageDescription { get; set; }

        // Financial Summary
        public decimal InitialDeposit { get; set; }
        public decimal TotalAdditionalCharges { get; set; }
        public decimal DepositRefundAmount { get; set; }
        public decimal AdditionalPaymentRequired { get; set; }

        // Status
        public bool IsFinalized { get; set; }
        public string? InvoiceUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? FinalizedAt { get; set; }

        // Calculated Properties
        public string SettlementSummary => AdditionalPaymentRequired > 0
            ? $"Additional payment required: {AdditionalPaymentRequired:N0} VND"
            : $"Deposit refund: {DepositRefundAmount:N0} VND";
    }
}
