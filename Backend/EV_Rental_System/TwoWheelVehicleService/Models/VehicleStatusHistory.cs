using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwoWheelVehicleService.Models
{
    [Table("VehicleStatusHistory")]
    public class VehicleStatusHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [Required]
        [Range(0, 100)]
        public int BatteryLevel { get; set; }

        [Required]
        public TechnicalStatus TechnicalStatus { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [Required]
        public int UpdatedBy { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [MaxLength(255)]
        public string? Location { get; set; }

        // Navigation properties
        [ForeignKey("VehicleId")]
        public virtual Vehicle? Vehicle { get; set; }
    }
}
