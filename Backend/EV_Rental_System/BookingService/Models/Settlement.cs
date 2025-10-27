using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BookingService.Models
{
    /// <summary>
    /// Represents the post-rental settlement/billing for an order.
    /// Calculates overtime fees, damage charges, and deposit refunds.
    /// </summary>
    public class Settlement
    {
        [Key]
        public int SettlementId { get; set; }

        // ===== Relationship =====
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        // ===== Timing Information =====
        public DateTime ScheduledReturnTime { get; set; }
        public DateTime ActualReturnTime { get; set; }

        // ===== Overtime Charges =====
        /// <summary>
        /// Number of hours late (after grace period)
        /// </summary>
        public decimal OvertimeHours { get; set; } = 0;

        /// <summary>
        /// Overtime fee = OvertimeHours × HourlyRate × OvertimeMultiplier
        /// </summary>
        public decimal OvertimeFee { get; set; } = 0;

        // ===== Damage Charges =====
        /// <summary>
        /// Total cost of damages to the vehicle
        /// </summary>
        public decimal DamageCharge { get; set; } = 0;

        /// <summary>
        /// Description of damages (optional)
        /// </summary>
        public string? DamageDescription { get; set; }

        // ===== Financial Summary =====
        /// <summary>
        /// Original deposit amount paid at booking
        /// </summary>
        public decimal InitialDeposit { get; set; }

        /// <summary>
        /// Total additional charges = OvertimeFee + DamageCharge
        /// </summary>
        public decimal TotalAdditionalCharges { get; set; } = 0;

        /// <summary>
        /// Amount to refund to customer = InitialDeposit - TotalAdditionalCharges
        /// (If positive, customer gets refund. If negative, customer owes money)
        /// </summary>
        public decimal DepositRefundAmount { get; set; } = 0;

        /// <summary>
        /// If TotalAdditionalCharges > InitialDeposit, this is the amount customer still owes
        /// </summary>
        public decimal AdditionalPaymentRequired { get; set; } = 0;

        // ===== Invoice & Status =====
        /// <summary>
        /// S3 URL to the settlement invoice PDF
        /// </summary>
        public string? InvoiceUrl { get; set; }

        /// <summary>
        /// Whether the settlement has been finalized (confirmed by staff)
        /// </summary>
        public bool IsFinalized { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? FinalizedAt { get; set; }

        // ===== Business Methods =====

        /// <summary>
        /// Calculate settlement totals based on charges
        /// </summary>
        public void CalculateTotals()
        {
            TotalAdditionalCharges = OvertimeFee + DamageCharge;

            decimal netAmount = InitialDeposit - TotalAdditionalCharges;

            if (netAmount >= 0)
            {
                // Customer gets refund
                DepositRefundAmount = netAmount;
                AdditionalPaymentRequired = 0;
            }
            else
            {
                // Customer owes additional money
                DepositRefundAmount = 0;
                AdditionalPaymentRequired = Math.Abs(netAmount);
            }
        }

        /// <summary>
        /// Finalize the settlement (lock it in)
        /// </summary>
        public void Complete()
        {
            if (IsFinalized)
                throw new InvalidOperationException($"Settlement {SettlementId} is already finalized.");

            IsFinalized = true;
            FinalizedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Calculate overtime hours with grace period
        /// </summary>
        public void CalculateOvertime(decimal hourlyRate, decimal overtimeMultiplier, int gracePeriodMinutes)
        {
            var lateDuration = ActualReturnTime - ScheduledReturnTime;

            if (lateDuration.TotalMinutes <= gracePeriodMinutes)
            {
                // Within grace period, no overtime
                OvertimeHours = 0;
                OvertimeFee = 0;
                return;
            }

            // Calculate overtime hours (subtract grace period)
            var overtimeMinutes = lateDuration.TotalMinutes - gracePeriodMinutes;
            OvertimeHours = (decimal)(overtimeMinutes / 60.0);

            // Apply overtime rate
            OvertimeFee = OvertimeHours * hourlyRate * overtimeMultiplier;
        }

        /// <summary>
        /// Add damage charge
        /// </summary>
        public void AddDamageCharge(decimal amount, string? description = null)
        {
            if (amount < 0)
                throw new ArgumentException("Damage charge cannot be negative", nameof(amount));

            DamageCharge += amount;

            if (!string.IsNullOrWhiteSpace(description))
            {
                DamageDescription = string.IsNullOrWhiteSpace(DamageDescription)
                    ? description
                    : $"{DamageDescription}; {description}";
            }
        }
    }
}
