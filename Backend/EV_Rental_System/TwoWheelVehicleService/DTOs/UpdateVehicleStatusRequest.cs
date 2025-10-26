using System.ComponentModel.DataAnnotations;

namespace TwoWheelVehicleService.DTOs
{
    public class UpdateVehicleStatusRequest
    {
        [Required(ErrorMessage = "Mức pin không được để trống")]
        [Range(0, 100, ErrorMessage = "Mức pin phải từ 0 đến 100")]
        public int BatteryLevel { get; set; }

        [Required(ErrorMessage = "Tình trạng kỹ thuật không được để trống")]
        [RegularExpression(@"^(good|fair|needs-check|needs-repair)$",
            ErrorMessage = "Tình trạng kỹ thuật không hợp lệ")]
        public string TechnicalStatus { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Ghi chú không được vượt quá 1000 ký tự")]
        public string? Notes { get; set; }
    }
}
