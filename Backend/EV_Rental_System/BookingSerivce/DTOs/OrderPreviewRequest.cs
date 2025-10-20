using System.ComponentModel.DataAnnotations;

namespace BookingSerivce.DTOs
{
    /// <summary>
    /// Request DTO for previewing an order before confirmation.
    /// Frontend sends this with basic booking info to get cost calculations and create a soft lock.
    /// </summary>
    public class OrderPreviewRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public DateTime ToDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Hourly rate must be greater than 0")]
        public decimal HourlyRate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Vehicle value must be greater than 0")]
        public decimal VehicleValue { get; set; }
    }
}
