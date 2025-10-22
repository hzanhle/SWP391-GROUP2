using System.ComponentModel.DataAnnotations;
using BookingService.Models.Enums;

namespace BookingService.DTOs.Inspection
{
    /// <summary>
    /// Request to create a new vehicle inspection
    /// </summary>
    public class CreateInspectionRequest
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [Required]
        public InspectionType InspectionType { get; set; }

        [Required]
        public int InspectorUserId { get; set; }

        [Range(0, int.MaxValue)]
        public int? Mileage { get; set; }

        [Range(0, 100)]
        public int? BatteryLevel { get; set; }

        public VehicleCondition? OverallCondition { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }
}
