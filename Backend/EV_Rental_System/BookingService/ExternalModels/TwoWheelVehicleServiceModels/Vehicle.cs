using System.ComponentModel.DataAnnotations;

namespace BookingService.ExternalModels.TwoWheelVehicleServiceModels
{
    public class Vehicle
    {
        [Key]
        public int VehicleId { get; set; }
        public int ModelId { get; set; }
        public int StationId { get; set; }
        public Model? Model { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string Status { get; set; } = string.Empty;
        public int CurrentBatteryLevel { get; set; }
    }

    public class Model
    {
        [Key]
        public int ModelId { get; set; }
        public string ModelName { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public int Year { get; set; }
        public bool IsActive { get; set; }
    }
}

