using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BookingService.Models.Enums;

namespace BookingService.Models
{
    /// <summary>
    /// Vehicle inspection record (pickup or return)
    /// </summary>
    public class VehicleInspection
    {
        [Key]
        public int InspectionId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public InspectionType InspectionType { get; set; }

        /// <summary>
        /// ID of the user who performed the inspection (employee or member)
        /// </summary>
        [Required]
        public int InspectorUserId { get; set; }

        [Required]
        public DateTime InspectionDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Vehicle mileage/odometer reading at inspection time
        /// </summary>
        public int? Mileage { get; set; }

        /// <summary>
        /// Battery level (0-100%) for electric vehicles
        /// </summary>
        [Range(0, 100)]
        public int? BatteryLevel { get; set; }

        /// <summary>
        /// Overall condition assessment
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public VehicleCondition? OverallCondition { get; set; }

        /// <summary>
        /// General notes about the inspection
        /// </summary>
        [MaxLength(2000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(OrderId))]
        public Order? Order { get; set; }

        public ICollection<InspectionDamage> Damages { get; set; } = new List<InspectionDamage>();
        public ICollection<InspectionPhoto> Photos { get; set; } = new List<InspectionPhoto>();
    }
}
