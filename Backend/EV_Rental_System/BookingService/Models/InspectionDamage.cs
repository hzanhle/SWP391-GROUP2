using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BookingService.Models.Enums;

namespace BookingService.Models
{
    /// <summary>
    /// Record of damage found during vehicle inspection
    /// </summary>
    public class InspectionDamage
    {
        [Key]
        public int DamageId { get; set; }

        [Required]
        public int InspectionId { get; set; }

        /// <summary>
        /// Type of damage (e.g., "Scratch", "Dent", "Broken Mirror", etc.)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string DamageType { get; set; } = string.Empty;

        /// <summary>
        /// Location of damage on vehicle (e.g., "Front Bumper", "Left Door", "Dashboard")
        /// </summary>
        [MaxLength(100)]
        public string? Location { get; set; }

        /// <summary>
        /// Severity of the damage
        /// </summary>
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DamageSeverity Severity { get; set; }

        /// <summary>
        /// Detailed description of the damage
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Estimated cost to repair (in VND)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? EstimatedCost { get; set; }

        /// <summary>
        /// URL/path to photo of the damage
        /// </summary>
        [MaxLength(500)]
        public string? PhotoUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey(nameof(InspectionId))]
        public VehicleInspection? Inspection { get; set; }
    }
}
