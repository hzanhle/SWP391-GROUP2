namespace TwoWheelVehicleService.DTOs
{
    /// <summary>
    /// Detailed vehicle information with availability status
    /// </summary>
    public class VehicleAvailabilityDto
    {
        public int VehicleId { get; set; }
        public int ModelId { get; set; }
        public string ModelName { get; set; }
        public string Manufacturer { get; set; }
        public string Color { get; set; }
        public int StationId { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; }
        public bool IsAvailableForDates { get; set; }
        public double RentFeeForHour { get; set; }
        public double ModelCost { get; set; }
    }
}
