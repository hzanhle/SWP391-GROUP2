namespace BookingService.DTOs
{
    /// <summary>
    /// OUTPUT for CreateOrderAsync. Confirms order creation and
    /// provides details needed for the payment step.
    /// </summary>
    public class OrderResponse
    {
        public int OrderId { get; set; }
        public string Status { get; set; } // Should always be "Pending"
        public decimal TotalAmount { get; set; } // Final amount calculated by BE
        public DateTime? ExpiresAt { get; set; } // Payment deadline for UI countdown
        public string Message { get; set; }
    }
}