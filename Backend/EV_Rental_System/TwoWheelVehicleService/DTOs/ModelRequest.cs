namespace TwoWheelVehicleService.DTOs
{
    public class ModelRequest
    {
        public List<IFormFile>? Files { get; set; }
        public string ModelName { get; set; }
        public string Manufacturer { get; set; }
        public int Year { get; set; }
        public int MaxSpeed { get; set; }
        public int BatteryCapacity { get; set; }
        public int ChargingTime { get; set; }
        public int BatteryRange { get; set; }
        public int VehicleCapacity { get; set; }
        public double ModelCost { get; set; } // Giá thành của mẫu xe
        public double RentFeeForHour { get; set; } // Giá thuê theo giờ
    }
}
