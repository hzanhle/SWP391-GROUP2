using TwoWheelVehicleService.Models;
using TwoWheelVehicleService.Repositories;

namespace TwoWheelVehicleService.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IVehicleRepository _vehicleRepository;

        public VehicleService(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }
        public async Task AddVehicleAsync(Vehicle vehicle)
        {
            await _vehicleRepository.AddVehicle(vehicle);

        }

        public async Task DeleteVehicleAsync(int vehicleId)
        {
            await _vehicleRepository.DeleteVehicle(vehicleId);
        }

        public async Task<List<Vehicle>> GetActiveVehiclesAsync()
        {
            return await _vehicleRepository.GetActiveVehicles();
        }

        public async Task<List<Vehicle>> GetAllVehiclesAsync()
        {
            return await _vehicleRepository.GetAllVehicles();
        }

        public async Task<Vehicle> GetVehicleByIdAsync(int vehicleId)
        {
            return await _vehicleRepository.GetVehicleById(vehicleId);
        }

        public async Task SetVehicleStatus(int vehicleId, string status)
        {
            await _vehicleRepository.ChangeStatus(vehicleId, status);
        }

        public async Task UpdateVehicleAsync(Vehicle vehicle)
        {
            var existingVehicle = await _vehicleRepository.GetVehicleById(vehicle.VehicleId);
            if (existingVehicle != null)
            {
                existingVehicle.Model = vehicle.Model;
                existingVehicle.Color = vehicle.Color;
                existingVehicle.Status = vehicle.Status;
                existingVehicle.IsActive = vehicle.IsActive;
                await _vehicleRepository.UpdateVehicle(existingVehicle); // Using AddVehicle to save changes
            }
        }
    }
}
