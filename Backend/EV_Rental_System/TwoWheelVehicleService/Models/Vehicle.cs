using System.ComponentModel.DataAnnotations;

namespace TwoWheelVehicleService.Models
{
    public class Vehicle
    {
        [Key]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "ModelId không được để trống")]
        public int ModelId { get; set; }

        [Required(ErrorMessage = "StationId không được để trống")]
        public int StationId { get; set; }

        public Model? Model { get; set; }

        [Required(ErrorMessage = "Màu xe không được để trống")]
        [StringLength(30, ErrorMessage = "Màu xe không được vượt quá 30 ký tự")]
        public string Color { get; set; }

        public bool IsActive { get; set; }

        [Required(ErrorMessage = "Trạng thái không được để trống")]
        [StringLength(20, ErrorMessage = "Trạng thái không được vượt quá 20 ký tự")]
        public string Status { get; set; }

        [Range(0, 100)]
        public int CurrentBatteryLevel { get; set; } = 100;

        public TechnicalStatus CurrentTechnicalStatus { get; set; } = TechnicalStatus.Good;

        public DateTime? LastStatusUpdate { get; set; }

        public int? LastUpdatedBy { get; set; }

        public Vehicle() { }

        public Vehicle(int vehicleId, int modelId, int stationId, Model? model, string color)
        {
            VehicleId = vehicleId;
            ModelId = modelId;
            StationId = stationId;
            Model = model;
            Color = color;
        }
    }
}
