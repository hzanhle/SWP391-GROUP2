namespace BookingService.Models.ModelSettings
{
    /// <summary>
    /// Configuration settings for billing and settlement calculations
    /// </summary>
    public class BillingSettings
    {
        /// <summary>
        /// Grace period in minutes before overtime charges apply
        /// Default: 15 minutes
        /// </summary>
        public int OvertimeGracePeriodMinutes { get; set; } = 15;

        /// <summary>
        /// Multiplier for overtime hourly rate
        /// Default: 1.5 (150% of normal rate)
        /// </summary>
        public decimal OvertimeRateMultiplier { get; set; } = 1.5m;

        /// <summary>
        /// Whether to allow negative balance (customer owes money beyond deposit)
        /// Default: true
        /// </summary>
        public bool AllowNegativeBalance { get; set; } = true;

        /// <summary>
        /// Whether to automatically generate invoice upon settlement finalization
        /// Default: true
        /// </summary>
        public bool AutoGenerateInvoice { get; set; } = true;

        /// <summary>
        /// Prefix for invoice numbers
        /// Default: "INV"
        /// </summary>
        public string InvoiceNumberPrefix { get; set; } = "INV";

        // ===== Trust Score Penalty Settings =====

        /// <summary>
        /// Trust score penalty per hour of late return (after grace period)
        /// Default: 5 points per hour
        /// </summary>
        public int LateReturnPenaltyPerHour { get; set; } = 5;

        /// <summary>
        /// Trust score penalty for minor vehicle damage
        /// Default: 10 points
        /// </summary>
        public int MinorDamagePenalty { get; set; } = 10;

        /// <summary>
        /// Damage amount threshold (VND) to distinguish minor vs major damage
        /// Default: 1,000,000 VND
        /// </summary>
        public decimal MajorDamageThreshold { get; set; } = 1000000m;

        /// <summary>
        /// Trust score penalty for major vehicle damage (>= threshold)
        /// Default: 30 points
        /// </summary>
        public int MajorDamagePenalty { get; set; } = 30;
    }
}
