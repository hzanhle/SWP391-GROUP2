namespace TwoWheelVehicleService.DTOs
{
    /// <summary>
    /// Response indicating whether a vehicle is available for the requested date range
    /// </summary>
    public class AvailabilityCheckResponse
    {
        public int VehicleId { get; set; }
        public bool IsAvailable { get; set; }
        public string Message { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<ConflictingOrderDto> ConflictingOrders { get; set; } = new();
    }

    public class ConflictingOrderDto
    {
        public int OrderId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Status { get; set; }
    }
}
