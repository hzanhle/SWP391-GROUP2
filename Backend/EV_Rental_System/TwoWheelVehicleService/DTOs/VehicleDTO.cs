namespace TwoWheelVehicleService.DTOs
{
    public class VehicleDTO
    {
        public int VehicleId { get; set; }
        public ModelDTO? Model { get; set; }
        public string Color { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; }
    }
}