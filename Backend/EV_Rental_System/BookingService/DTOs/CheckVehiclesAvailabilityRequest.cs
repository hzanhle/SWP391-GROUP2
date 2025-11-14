namespace BookingService.DTOs
{
    /// <summary>
    /// Request to check availability of multiple vehicles for a date range
    /// </summary>
    public class CheckVehiclesAvailabilityRequest
    {
        public List<int> VehicleIds { get; set; } = new();
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    /// <summary>
    /// Response for vehicle availability check
    /// </summary>
    public class VehicleAvailabilityResponse
    {
        public int VehicleId { get; set; }
        public bool IsAvailable { get; set; }
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Response for multiple vehicles availability check
    /// </summary>
    public class CheckVehiclesAvailabilityResponse
    {
        public List<VehicleAvailabilityResponse> Results { get; set; } = new();
        public List<int> AvailableVehicleIds { get; set; } = new();
    }
}

