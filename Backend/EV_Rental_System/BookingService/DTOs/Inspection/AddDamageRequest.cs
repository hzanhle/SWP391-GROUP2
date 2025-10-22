using System.ComponentModel.DataAnnotations;
using BookingService.Models.Enums;

namespace BookingService.DTOs.Inspection
{
    /// <summary>
    /// Request to add a damage record to an inspection
    /// </summary>
    public class AddDamageRequest
    {
        [Required]
        [MaxLength(50)]
        public string DamageType { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Location { get; set; }

        [Required]
        public DamageSeverity Severity { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? EstimatedCost { get; set; }

        [MaxLength(500)]
        public string? PhotoUrl { get; set; }
    }
}
