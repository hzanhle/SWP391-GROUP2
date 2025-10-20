namespace BookingSerivce.DTOs
{
    /// <summary>
    /// Response DTO returned after order confirmation.
    /// Contains the created OrderId and payment details for frontend to display.
    /// </summary>
    public class OrderResponse
    {
        public int OrderId { get; set; }

        public decimal TotalCost { get; set; }

        public decimal DepositAmount { get; set; }

        public DateTime ExpiresAt { get; set; } // When payment must be initiated by (5 minutes)

        public string Status { get; set; } = "Pending";

        public int TrustScore { get; set; }

        public decimal DepositPercentage { get; set; }
    }
}
