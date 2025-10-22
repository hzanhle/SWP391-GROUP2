using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingService.Models
{
    /// <summary>
    /// Photos taken during vehicle inspection
    /// </summary>
    public class InspectionPhoto
    {
        [Key]
        public int PhotoId { get; set; }

        [Required]
        public int InspectionId { get; set; }

        /// <summary>
        /// URL/path to the photo file
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string PhotoUrl { get; set; } = string.Empty;

        /// <summary>
        /// Type/angle of photo (e.g., "Front", "Back", "Left", "Right", "Dashboard", "Damage")
        /// </summary>
        [MaxLength(50)]
        public string? PhotoType { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey(nameof(InspectionId))]
        public VehicleInspection? Inspection { get; set; }
    }
}
