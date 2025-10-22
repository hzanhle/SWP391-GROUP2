using TwoWheelVehicleService.DTOs;

namespace TwoWheelVehicleService.Services
{
    public interface IAvailabilityService
    {
        /// <summary>
        /// Check if a specific vehicle is available for the given date range
        /// </summary>
        Task<AvailabilityCheckResponse> CheckVehicleAvailabilityAsync(AvailabilityCheckRequest request);

        /// <summary>
        /// Get all available vehicles for a specific station and date range
        /// </summary>
        Task<List<VehicleAvailabilityDto>> GetAvailableVehiclesByStationAsync(int stationId, DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Get all available vehicles (any station) for a date range
        /// </summary>
        Task<List<VehicleAvailabilityDto>> GetAllAvailableVehiclesAsync(DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Bulk check availability for multiple vehicles
        /// </summary>
        Task<Dictionary<int, bool>> BulkCheckAvailabilityAsync(List<int> vehicleIds, DateTime fromDate, DateTime toDate);
    }
}
