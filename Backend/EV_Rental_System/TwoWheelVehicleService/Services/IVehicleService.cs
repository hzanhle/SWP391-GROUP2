﻿using TwoWheelVehicleService.Models;

namespace TwoWheelVehicleService.Services
{
    public interface IVehicleService
    {
        Task<List<Vehicle>> GetAllVehiclesAsync();
        Task<List<Vehicle>> GetActiveVehiclesAsync();
        Task AddVehicleAsync(Vehicle vehicle);
        Task UpdateVehicleAsync(Vehicle vehicle);
        Task DeleteVehicleAsync(int vehicleId); // Soft delete
        Task<Vehicle> GetVehicleByIdAsync(int vehicleId);
        Task SetVehicleStatus(int vehicleId, string status); // New method to set vehicle status
    }
}
