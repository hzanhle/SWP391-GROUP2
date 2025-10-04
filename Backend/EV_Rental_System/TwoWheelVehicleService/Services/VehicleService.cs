﻿using TwoWheelVehicleService.DTOs;
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
        public async Task AddVehicleAsync(VehicleRequest vehicle)
        {
            var newVehicle = new Vehicle
            {
                ModelId = vehicle.ModelId,
                StationId = vehicle.StationId,
                Color = vehicle.Color,
                Status = "Available", // Default status
                IsActive = true // New vehicles are active by default
            };
            
            await _vehicleRepository.AddVehicle(newVehicle);

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

        public async Task<VehicleDTO> GetVehicleByIdAsync(int vehicleId)
        {
            var vehicle = await _vehicleRepository.GetVehicleById(vehicleId);
            var vehicleDTO = new VehicleDTO
            {
                VehicleId = vehicle.VehicleId,
                ModelId = vehicle.ModelId,
                StationId = vehicle.StationId,
                Color = vehicle.Color,
                Status = vehicle.Status,
                IsActive = vehicle.IsActive
            };
            return vehicleDTO;
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
                existingVehicle.ModelId = vehicle.ModelId;
                existingVehicle.StationId = vehicle.StationId;
                existingVehicle.Color = vehicle.Color;
                existingVehicle.Status = vehicle.Status;
                existingVehicle.IsActive = vehicle.IsActive;
                await _vehicleRepository.UpdateVehicle(existingVehicle); 
            }
        }
    }
}
