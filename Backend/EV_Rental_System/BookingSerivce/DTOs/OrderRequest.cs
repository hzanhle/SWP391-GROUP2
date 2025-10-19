using System.ComponentModel.DataAnnotations;

namespace BookingService.DTOs
{
    /// <summary>
    /// INPUT for GetOrderPreviewAsync. Basic info + base pricing data.
    /// </summary>
    public class OrderRequest
    {
        public int UserId { get; set; }
        public int VehicleId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        // Data from FE/Gateway needed for BE calculation
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Rent fee must be positive.")]
        public decimal RentFeeForHour { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Model price must be positive.")]
        public decimal ModelPrice { get; set; } // Needed to calculate deposit
    }
}