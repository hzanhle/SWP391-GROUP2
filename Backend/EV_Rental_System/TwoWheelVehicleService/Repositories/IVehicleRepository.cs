using TwoWheelVehicleService.DTOs;
using TwoWheelVehicleService.Models;

namespace TwoWheelVehicleService.Repositories
{
    public interface IVehicleRepository
    {
        Task<List<Vehicle>> GetVehiclesByStatus(string status); // Get Vehicles by Status
        Task<VehicleDTO?> GetAvailableVehicleAsync(int modelId, string color, int stationId);
        Task<List<Vehicle>> GetAllVehiclesByModelId(int modelId); // Get Vehicles by ModelId
        Task<List<Vehicle>> GetActiveVehicles(); // Get Active Vehicles
        Task<List<Vehicle>> GetAllVehicles(); // Get All Vehicles
        Task AddVehicle(Vehicle vehicle);
        Task<Vehicle?> GetVehicleById(int Id);
        Task UpdateVehicle(Vehicle vehicle);
        Task DeleteVehicle(int vehicleId); // Delete 
        //Update cho phần quản lý phương tiện cho staff
        Task<List<VehicleStatusHistory>> GetVehicleHistoryAsync(int vehicleId);
        Task<VehicleStatusHistory?> GetLatestStatusAsync(int vehicleId);
        Task<VehicleStatusHistory> CreateHistoryAsync(VehicleStatusHistory history);
        Task<List<Vehicle>> GetVehiclesWithFiltersAsync(
            string? statusFilter,
            string? batteryFilter,
            string? search,
            int? stationId);

    }
}
