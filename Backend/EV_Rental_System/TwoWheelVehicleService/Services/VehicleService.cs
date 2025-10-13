using Microsoft.Extensions.Logging;
using TwoWheelVehicleService.DTOs;
using TwoWheelVehicleService.Models;
using TwoWheelVehicleService.Repositories;

namespace TwoWheelVehicleService.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ILogger<VehicleService> _logger;

        public VehicleService(IVehicleRepository vehicleRepository, ILogger<VehicleService> logger)
        {
            _vehicleRepository = vehicleRepository;
            _logger = logger;
        }

        public async Task AddVehicleAsync(VehicleRequest vehicle)
        {
            try
            {
                var newVehicle = new Vehicle
                {
                    ModelId = vehicle.ModelId,
                    StationId = vehicle.StationId,
                    Color = vehicle.Color,
                    Status = "Available",
                    IsActive = true
                };

                await _vehicleRepository.AddVehicle(newVehicle);
                _logger.LogInformation("✅ Vehicle added successfully: {@Vehicle}", newVehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error while adding vehicle: {@VehicleRequest}", vehicle);
                throw;
            }
        }

        public async Task DeleteVehicleAsync(int vehicleId)
        {
            try
            {
                await _vehicleRepository.DeleteVehicle(vehicleId);
                _logger.LogInformation("✅ Vehicle deleted successfully: ID={VehicleId}", vehicleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error while deleting vehicle ID={VehicleId}", vehicleId);
                throw;
            }
        }

        public async Task<List<Vehicle>> GetActiveVehiclesAsync()
        {
            try
            {
                var result = await _vehicleRepository.GetActiveVehicles();
                _logger.LogInformation("📗 Retrieved {Count} active vehicles", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error while retrieving active vehicles");
                throw;
            }
        }

        public async Task<List<Vehicle>> GetAllVehiclesAsync()
        {
            try
            {
                var result = await _vehicleRepository.GetAllVehicles();
                _logger.LogInformation("📘 Retrieved {Count} vehicles in total", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error while retrieving all vehicles");
                throw;
            }
        }

        public async Task<List<Vehicle>> GetAllVehiclesByModelId(int modelId)
        {
            try
            {
                var result = await _vehicleRepository.GetAllVehiclesByModelId(modelId);
                _logger.LogInformation("📗 Retrieved {Count} vehicles for ModelId={ModelId}", result.Count, modelId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error while retrieving vehicles by ModelId={ModelId}", modelId);
                throw;
            }
        }

        public async Task<VehicleDTO> GetVehicleByIdAsync(int vehicleId)
        {
            try
            {
                var vehicle = await _vehicleRepository.GetVehicleById(vehicleId);
                if (vehicle == null)
                {
                    _logger.LogWarning("⚠ Vehicle not found with ID={VehicleId}", vehicleId);
                    return null;
                }

                var vehicleDTO = new VehicleDTO
                {
                    VehicleId = vehicle.VehicleId,
                    ModelId = vehicle.ModelId,
                    StationId = vehicle.StationId,
                    Color = vehicle.Color,
                    Status = vehicle.Status,
                    IsActive = vehicle.IsActive
                };

                _logger.LogInformation("📘 Retrieved vehicle details: {@VehicleDTO}", vehicleDTO);
                return vehicleDTO;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error while retrieving vehicle ID={VehicleId}", vehicleId);
                throw;
            }
        }

        public async Task SetVehicleStatus(int vehicleId, string status)
        {
            try
            {
                var vehicle = await _vehicleRepository.GetVehicleById(vehicleId);
                if (vehicle == null)
                {
                    _logger.LogWarning("⚠ Cannot update status — Vehicle not found ID={VehicleId}", vehicleId);
                    return;
                }

                vehicle.Status = status;
                await _vehicleRepository.UpdateVehicle(vehicle);
                _logger.LogInformation("✅ Vehicle status updated: ID={VehicleId}, Status={Status}", vehicleId, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error while updating vehicle status ID={VehicleId}", vehicleId);
                throw;
            }
        }

        public async Task ToggleActiveStatus(int id)
        {
            try
            {
                var vehicle = await _vehicleRepository.GetVehicleById(id);
                if (vehicle == null)
                {
                    _logger.LogWarning("⚠ Cannot toggle active status — Vehicle not found ID={VehicleId}", id);
                    return;
                }

                vehicle.IsActive = !vehicle.IsActive;
                await _vehicleRepository.UpdateVehicle(vehicle);
                _logger.LogInformation("✅ Vehicle IsActive toggled: ID={VehicleId}, NewValue={IsActive}", id, vehicle.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error while toggling active status for vehicle ID={VehicleId}", id);
                throw;
            }
        }

        public async Task UpdateVehicleAsync(Vehicle vehicle)
        {
            try
            {
                var existingVehicle = await _vehicleRepository.GetVehicleById(vehicle.VehicleId);
                if (existingVehicle == null)
                {
                    _logger.LogWarning("⚠ Vehicle not found for update: ID={VehicleId}", vehicle.VehicleId);
                    return;
                }

                existingVehicle.ModelId = vehicle.ModelId;
                existingVehicle.StationId = vehicle.StationId;
                existingVehicle.Color = vehicle.Color;
                existingVehicle.Status = vehicle.Status;
                existingVehicle.IsActive = vehicle.IsActive;

                await _vehicleRepository.UpdateVehicle(existingVehicle);
                _logger.LogInformation("✅ Vehicle updated successfully: {@Vehicle}", existingVehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error while updating vehicle: {@Vehicle}", vehicle);
                throw;
            }
        }
    }
}
