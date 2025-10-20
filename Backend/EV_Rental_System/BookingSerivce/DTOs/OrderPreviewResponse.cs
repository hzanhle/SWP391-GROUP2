namespace BookingSerivce.DTOs
{
    /// <summary>
    /// Response DTO returned after order preview calculation.
    /// Contains the preview token (soft lock), calculated costs, and expiration time.
    /// Frontend stores this and uses PreviewToken when confirming the order.
    /// </summary>
    public class OrderPreviewResponse
    {
        public Guid PreviewToken { get; set; } // Token linking to the SoftLock

        public decimal TotalCost { get; set; } // Total rental cost

        public decimal DepositAmount { get; set; } // Calculated deposit amount

        public decimal DepositPercentage { get; set; } // Actual percentage used (0.30, 0.40, or 0.50)

        public int TrustScore { get; set; } // User's current trust score

        public DateTime ExpiresAt { get; set; } // When this preview expires (5 minutes from creation)

        public int TotalDays { get; set; } // Total rental days

        public decimal ModelPrice { get; set; } // Vehicle hourly rate (stored for record)
    }
}
