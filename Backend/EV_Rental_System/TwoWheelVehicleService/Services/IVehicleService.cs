using TwoWheelVehicleService.DTOs;
using TwoWheelVehicleService.Models;

namespace TwoWheelVehicleService.Services
{
    public interface IVehicleService
    {
        Task<List<Vehicle>> GetAllVehiclesByModelId(int modelId);
        Task<List<Vehicle>> GetAllVehiclesAsync();
        Task<List<Vehicle>> GetActiveVehiclesAsync();
        Task AddVehicleAsync(VehicleRequest vehicle);
        Task UpdateVehicleAsync(Vehicle vehicle);
        Task DeleteVehicleAsync(int vehicleId);
        Task<VehicleDTO> GetVehicleByIdAsync(int vehicleId);
        Task SetVehicleStatus(int vehicleId, string status); // New method to set vehicle status
        Task ToggleActiveStatus(int id); // New method to toggle isActive status

        Task<VehicleStatusResponse> UpdateVehicleStatusAsync(
                                        int vehicleId,
                                        UpdateVehicleStatusRequest request,
                                        int staffId);
        Task<List<VehicleStatusResponse>> GetVehiclesWithStatusAsync(
                                            string? statusFilter,
                                            string? batteryFilter,
                                            string? search,
                                            int? stationId);
        Task<VehicleHistoryDTO> GetVehicleHistoryAsync(int vehicleId);

        Task<VehicleStatsResponse> GetVehicleStatsAsync(int? stationId);
    }
}
