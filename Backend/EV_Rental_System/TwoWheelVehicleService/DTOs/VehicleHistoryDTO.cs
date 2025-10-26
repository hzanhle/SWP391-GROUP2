// DTOs/VehicleHistoryDto.cs
// Tạo file mới này trong thư mục DTOs của project
using TwoWheelVehicleService.Models;

namespace TwoWheelVehicleService.DTOs
{
    public class VehicleHistoryDTO
    {
        public int VehicleId { get; set; }
        public string Color { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public int CurrentBatteryLevel { get; set; }
        public TechnicalStatus CurrentTechnicalStatus { get; set; }
        public DateTime? LastStatusUpdate { get; set; }
        public int StationId { get; set; }
        public VehicleModelDTO Model { get; set; }
        public List<VehicleStatusHistoryDTO> StatusHistory { get; set; }
    }

    public class VehicleModelDTO
    {
        public int ModelId { get; set; }
        public string ModelName { get; set; }
        public string Manufacturer { get; set; }
        public int Year { get; set; }
        public int MaxSpeed { get; set; }
        public int BatteryCapacity { get; set; }
        public int ChargingTime { get; set; }
        public int BatteryRange { get; set; }
        public int VehicleCapacity { get; set; }
        public double ModelCost { get; set; }
        public double RentFeeForHour { get; set; }
    }

    public class VehicleStatusHistoryDTO
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public int BatteryLevel { get; set; }
        public TechnicalStatus TechnicalStatus { get; set; }
        public string Notes { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Location { get; set; }
    }
}