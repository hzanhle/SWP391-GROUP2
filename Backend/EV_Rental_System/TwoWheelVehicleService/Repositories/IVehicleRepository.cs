using TwoWheelVehicleService.Models;

namespace TwoWheelVehicleService.Repositories
{
    public interface IVehicleRepository
    {
        Task<List<Vehicle>> GetVehiclesByStatus(string status); // Get Vehicles by Status
        Task<List<Vehicle>> GetAllVehiclesByModelId(int modelId); // Get Vehicles by ModelId
        Task<List<Vehicle>> GetActiveVehicles(); // Get Active Vehicles
        Task<List<Vehicle>> GetAllVehicles(); // Get All Vehicles
        Task AddVehicle(Vehicle vehicle);
        Task<Vehicle> GetVehicleById(int Id);
        Task UpdateVehicle(Vehicle vehicle);
        Task DeleteVehicle(int vehicleId); // Delete 
        
    }
}
