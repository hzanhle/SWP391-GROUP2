using System.ComponentModel.DataAnnotations;

namespace AdminDashboardService.ExternalModels.TwoWheelVehicleServiceModels
{
    public class Vehicle
    {
        [Key]
        public int VehicleId { get; set; }
        public int ModelId { get; set; }
        public int StationId { get; set; }
        public Model? Model { get; set; }
        public string Color { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class Model
    {
        [Key]
        public int ModelId { get; set; }
        public string ModelName { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public int Year { get; set; }
        public int MaxSpeed { get; set; }
        public int BatteryCapacity { get; set; }
        public int ChargingTime { get; set; }
        public int BatteryRange { get; set; }
        public int VehicleCapacity { get; set; }
        public bool IsActive { get; set; }
        public double ModelCost { get; set; }
        public double RentFeeForHour { get; set; }
    }

    public class Image
    {
        [Key]
        public int ImageId { get; set; }
        public string Url { get; set; } = string.Empty;
        public int ModelId { get; set; }
    }
}