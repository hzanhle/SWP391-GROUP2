namespace TwoWheelVehicleService.DTOs
{
    public class VehicleDTO
    {
        public int VehicleId { get; set; }
        public int? StationId { get; set; }
        public int? ModelId { get; set; }
        public string Color { get; set; }
        public bool? IsActive { get; set; }
        public string? Status { get; set; }
    }
}