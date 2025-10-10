using System.ComponentModel.DataAnnotations;

namespace TwoWheelVehicleService.DTOs
{
    public class VehicleRequest
    {
        [Required(ErrorMessage = "ModelId không được để trống")]
        public int ModelId { get; set; }
        [Required(ErrorMessage = "StationId không được để trống")]
        public int StationId { get; set; }
        [Required(ErrorMessage = "Màu xe không được để trống")]
        [StringLength(30, ErrorMessage = "Màu xe không được vượt quá 30 ký tự")]
        public string Color { get; set; }

    }
}
