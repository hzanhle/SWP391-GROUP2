namespace BookingService.DTOs
{
    /// <summary>
    /// Single rental history item in user's rental history list
    /// </summary>
    public class RentalHistoryItemResponse
    {
        public int OrderId { get; set; }
        public int VehicleId { get; set; }
        public string VehicleName { get; set; } = string.Empty; // Fetched from TwoWheelVehicleService
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime? ActualReturnTime { get; set; }
        public decimal TotalCost { get; set; }
        public decimal DepositAmount { get; set; }
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Whether the rental was returned late
        /// </summary>
        public bool IsLate { get; set; }

        /// <summary>
        /// Whether there were damage charges
        /// </summary>
        public bool HasDamage { get; set; }

        /// <summary>
        /// Trust score impact from this rental
        /// </summary>
        public int TrustScoreImpact { get; set; }

        /// <summary>
        /// Settlement details (if exists)
        /// </summary>
        public decimal? OvertimeFee { get; set; }
        public decimal? DamageCharge { get; set; }
        public decimal? DepositRefundAmount { get; set; }
        public decimal? AdditionalPaymentRequired { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
