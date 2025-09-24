namespace TwoWheelVehicleService.DTOs
{
    public class ModelDTO
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
        public bool IsActive { get; set; }
        public int Price { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}
