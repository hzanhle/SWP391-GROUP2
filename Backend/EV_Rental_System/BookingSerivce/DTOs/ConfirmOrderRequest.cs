using System.ComponentModel.DataAnnotations;

namespace BookingSerivce.DTOs
{
    /// <summary>
    /// Request DTO for confirming an order after preview.
    /// Must include the PreviewToken received from the preview step.
    /// All data is validated against the original soft lock.
    /// </summary>
    public class ConfirmOrderRequest
    {
        [Required]
        public Guid PreviewToken { get; set; } // Token from OrderPreviewResponse

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
        [Range(0.01, double.MaxValue, ErrorMessage = "Total cost must be greater than 0")]
        public decimal TotalCost { get; set; }
    }
}
