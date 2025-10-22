using System.ComponentModel.DataAnnotations;

namespace TwoWheelVehicleService.DTOs
{
    /// <summary>
    /// Request to check vehicle availability for a specific date range
    /// </summary>
    public class AvailabilityCheckRequest
    {
        [Required(ErrorMessage = "Vehicle ID is required")]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "From date is required")]
        public DateTime FromDate { get; set; }

        [Required(ErrorMessage = "To date is required")]
        public DateTime ToDate { get; set; }

        public int? ExcludeOrderId { get; set; } // For checking when modifying existing order
    }
}
