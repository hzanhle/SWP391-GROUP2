using System.ComponentModel.DataAnnotations;

namespace BookingSerivce.DTOs
{
    public class ConfirmReturnRequest
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public int StaffId { get; set; }

        [Required]
        public DateTime ActualReturnTime { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int OdometerReading { get; set; }

        [Required]
        [Range(0, 100)]
        public int BatteryLevel { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public List<string>? PhotoUrls { get; set; } // URLs to photos of vehicle condition at return
    }
}
