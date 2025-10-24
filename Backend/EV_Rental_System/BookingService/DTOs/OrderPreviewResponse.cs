namespace BookingService.DTOs
{
    /// <summary>
    /// OUTPUT for GetOrderPreviewAsync. Contains BE-calculated costs.
    /// FE will merge this with User/Vehicle data for display.
    /// </summary>
    public class OrderPreviewResponse
    {
        // Basic Info (echoed back)
        public int UserId { get; set; }
        public int VehicleId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        // --- BE Calculated Costs ---
        public decimal TotalRentalCost { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal ServiceFee { get; set; }
        public decimal TotalPaymentAmount { get; set; } // Grand Total

        // --- Status ---
        public bool IsAvailable { get; set; } // Based on basic overlap check
        public string Message { get; set; }
    }
}