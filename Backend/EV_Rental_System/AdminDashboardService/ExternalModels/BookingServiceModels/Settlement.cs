using System.ComponentModel.DataAnnotations;

namespace AdminDashboardService.ExternalModels.BookingServiceModels
{
    /// <summary>
    /// External model for Settlement from BookingService (read-only)
    /// </summary>
    public class Settlement
    {
        [Key]
        public int SettlementId { get; set; }

        public int OrderId { get; set; }

        // Timing Information
        public DateTime ScheduledReturnTime { get; set; }
        public DateTime ActualReturnTime { get; set; }

        // Overtime Charges
        public decimal OvertimeHours { get; set; }
        public decimal OvertimeFee { get; set; }

        // Damage Charges
        public decimal DamageCharge { get; set; }
        public string? DamageDescription { get; set; }

        // Financial Summary
        public decimal InitialDeposit { get; set; }
        public decimal TotalAdditionalCharges { get; set; }
        public decimal DepositRefundAmount { get; set; }
        public decimal AdditionalPaymentRequired { get; set; }

        // Invoice & Status
        public string? InvoiceUrl { get; set; }
        public bool IsFinalized { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? FinalizedAt { get; set; }
    }
}
