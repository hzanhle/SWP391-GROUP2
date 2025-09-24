using System.ComponentModel.DataAnnotations;

namespace TwoWheelVehicleService.Models
{

    public class Model
    {
        [Key]
        public int ModelId { get; set; }
        public string ModelName { get; set; }
        public string Manufacturer { get; set; }
        public int Year { get; set; }
        public int MaxSpeed { get; set; }
        public int BatteryCapacity { get; set; }
        public int ChargingTime { get; set; }
        public int BatteryRange { get; set; }
        public int VehicleCapacity { get; set; }
        public bool IsActive { get; set; }
        public int Price { get; set; }

        // Navigation properties
        public ICollection<Image> Images { get; set; } = new List<Image>();
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

        public Model() { }

        public Model(string modelName, string manufacturer, int year, int maxSpeed, int batteryCapacity, int chargingTime, int batteryRange, int vehicleCapacity, bool isActive, int price)
        {
            ModelName = modelName;
            Manufacturer = manufacturer;
            Year = year;
            MaxSpeed = maxSpeed;
            BatteryCapacity = batteryCapacity;
            ChargingTime = chargingTime;
            BatteryRange = batteryRange;
            VehicleCapacity = vehicleCapacity;
            IsActive = isActive;
            Price = price;
        }
    }

}