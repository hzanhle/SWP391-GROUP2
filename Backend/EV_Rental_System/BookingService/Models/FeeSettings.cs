namespace BookingService.Models
{
    /// <summary>
    /// Configuration settings for automatic fee calculation
    /// </summary>
    public class FeeSettings
    {
        // Late Return Settings
        public int LateReturnGracePeriodMinutes { get; set; } = 60;
        public decimal LateReturnPenaltyMultiplier { get; set; } = 1.5m;

        // Damage Fee Multipliers
        public decimal DamageMinorMultiplier { get; set; } = 1.0m;
        public decimal DamageModerateMultiplier { get; set; } = 1.2m;
        public decimal DamageMajorMultiplier { get; set; } = 1.5m;

        // Mileage Settings
        public decimal ExcessMileageFeePerKm { get; set; } = 0.5m;
        public int IncludedKmPerDay { get; set; } = 100;
    }
}
