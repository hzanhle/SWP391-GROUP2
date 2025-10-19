namespace BookingService.DTOs
{
    public class RequestVehicle // dùng để chuyển dữ liệu xe giữa FE và BE, binding vào Contract
    {
        public int vehicleId { get; set; }

        public string ModelName { get; set; }

        public string LicensePlate { get; set; }

        public string VehicleType { get; set; }
    }
}
