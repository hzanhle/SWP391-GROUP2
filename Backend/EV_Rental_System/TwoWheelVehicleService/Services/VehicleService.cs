using Microsoft.Extensions.Logging;
using System.ComponentModel;
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

        public async Task<VehicleStatusResponse> UpdateVehicleStatusAsync(
            int vehicleId,
            UpdateVehicleStatusRequest request,
            int staffId)
        {
            try
            {
                // 1. Validate vehicle exists (FIX: Dùng GetVehicleById thay vì GetByIdAsync)
                var vehicle = await _vehicleRepository.GetVehicleById((int)vehicleId);
                if (vehicle == null)
                {
                    _logger.LogError($"Vehicle not found: {vehicleId}");
                    throw new Exception($"Vehicle not found: {vehicleId}");
                }

                _logger.LogInformation($"Staff {staffId} updating vehicle {vehicleId} status");

                // 2. Parse technical status
                var technicalStatus = ParseTechnicalStatus(request.TechnicalStatus);

                // 3. Update vehicle current status
                vehicle.CurrentBatteryLevel = request.BatteryLevel;
                vehicle.CurrentTechnicalStatus = technicalStatus;
                vehicle.LastStatusUpdate = DateTime.Now;
                vehicle.LastUpdatedBy = staffId;

                // 4. Auto-update vehicle status if needed
                if (technicalStatus == TechnicalStatus.NeedsRepair)
                {
                    vehicle.Status = "Maintenance";
                    _logger.LogInformation($"Auto-changed vehicle {vehicleId} status to MAINTENANCE");
                }
                else if (request.BatteryLevel < 30 && vehicle.Status == "Available")
                {
                    vehicle.Status = "Charging";
                    _logger.LogInformation($"Auto-changed vehicle {vehicleId} status to CHARGING");
                }

                // 5. Update vehicle
                await _vehicleRepository.UpdateVehicle(vehicle);

                // 6. Create history record
                var history = new VehicleStatusHistory
                {
                    VehicleId = vehicleId,
                    BatteryLevel = request.BatteryLevel,
                    TechnicalStatus = technicalStatus,
                    Notes = request.Notes,
                    UpdatedBy = staffId,
                    UpdatedAt = DateTime.Now
                };

                await _vehicleRepository.CreateHistoryAsync(history);

                // 7. TODO: If NEEDS_REPAIR, create incident report
                if (technicalStatus == TechnicalStatus.NeedsRepair)
                {
                    _logger.LogWarning($"Vehicle {vehicleId} marked as NEEDS_REPAIR - TODO: create incident");
                }

                _logger.LogInformation($"Successfully updated vehicle {vehicleId} status by staff {staffId}");

                return BuildVehicleStatusResponse(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating vehicle {vehicleId} status");
                throw;
            }
        }
        public async Task<List<VehicleStatusResponse>> GetVehiclesWithStatusAsync(
            string? statusFilter,
            string? batteryFilter,
            string? search,
            int? stationId)
        {
            try
            {
                var vehicles = await _vehicleRepository.GetVehiclesWithFiltersAsync(
                    statusFilter, batteryFilter, search, stationId);

                _logger.LogInformation($"Retrieved {vehicles.Count} vehicles with filters");

                return vehicles.Select(v => BuildVehicleStatusResponse(v)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching vehicles with status");
                throw;
            }
        }

        public async Task<VehicleHistoryDTO> GetVehicleHistoryAsync(int vehicleId)
        {
            try
            {
                // Validate vehicle exists
                var vehicle = await _vehicleRepository.GetVehicleById(vehicleId);
                if (vehicle == null)
                {
                    _logger.LogError($"Vehicle not found: {vehicleId}");
                    throw new Exception($"Vehicle not found: {vehicleId}");
                }

                // Get history records
                var historyRecords = await _vehicleRepository.GetVehicleHistoryAsync(vehicleId);

                _logger.LogInformation($"Retrieved {historyRecords.Count} history records for vehicle {vehicleId}");

                // Map to DTO to avoid circular reference
                var result = new VehicleHistoryDTO
                {
                    VehicleId = vehicle.VehicleId,
                    Color = vehicle.Color,
                    Status = vehicle.Status,
                    IsActive = vehicle.IsActive,
                    CurrentBatteryLevel = vehicle.CurrentBatteryLevel,
                    CurrentTechnicalStatus = vehicle.CurrentTechnicalStatus,
                    LastStatusUpdate = vehicle.LastStatusUpdate,
                    StationId = vehicle.StationId,
                    Model = vehicle.Model != null ? new VehicleModelDTO
                    {
                        ModelId = vehicle.Model.ModelId,
                        ModelName = vehicle.Model.ModelName,
                        Manufacturer = vehicle.Model.Manufacturer,
                        Year = vehicle.Model.Year,
                        MaxSpeed = vehicle.Model.MaxSpeed,
                        BatteryCapacity = vehicle.Model.BatteryCapacity,
                        ChargingTime = vehicle.Model.ChargingTime,
                        BatteryRange = vehicle.Model.BatteryRange,
                        VehicleCapacity = vehicle.Model.VehicleCapacity,
                        ModelCost = vehicle.Model.ModelCost,
                        RentFeeForHour = vehicle.Model.RentFeeForHour
                    } : null,
                    StatusHistory = historyRecords
                        .OrderByDescending(h => h.UpdatedAt)
                        .Select(h => new VehicleStatusHistoryDTO
                        {
                            Id = h.Id,
                            VehicleId = h.VehicleId,
                            BatteryLevel = h.BatteryLevel,
                            TechnicalStatus = h.TechnicalStatus,
                            Notes = h.Notes,
                            UpdatedBy = h.UpdatedBy,
                            UpdatedAt = h.UpdatedAt,
                            Location = h.Location
                        })
                        .ToList()
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching vehicle {vehicleId} history");
                throw;
            }
        }

        public async Task<VehicleStatsResponse> GetVehicleStatsAsync(int? stationId)
        {
            try
            {
                var vehicles = await _vehicleRepository.GetVehiclesWithFiltersAsync(
                    null, null, null, stationId);

                var stats = new VehicleStatsResponse
                {
                    Total = vehicles.Count,
                    Available = vehicles.Count(v => v.Status == "Available"),
                    InUse = vehicles.Count(v => v.Status == "InUse"),
                    Charging = vehicles.Count(v => v.Status == "Charging"),
                    NeedsAttention = vehicles.Count(v =>
                        v.CurrentBatteryLevel < 30 ||
                        v.CurrentTechnicalStatus == TechnicalStatus.NeedsCheck ||
                        v.CurrentTechnicalStatus == TechnicalStatus.NeedsRepair)
                };

                _logger.LogInformation($"Stats retrieved: {stats.Total} total, {stats.NeedsAttention} needs attention");

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating vehicle stats");
                throw;
            }
        }

        // ==================== HELPER METHODS ====================

        private TechnicalStatus ParseTechnicalStatus(string status)
        {
            return status.ToLower() switch
            {
                "good" => TechnicalStatus.Good,
                "fair" => TechnicalStatus.Fair,
                "needs-check" => TechnicalStatus.NeedsCheck,
                "needs-repair" => TechnicalStatus.NeedsRepair,
                _ => throw new ArgumentException($"Invalid technical status: {status}")
            };
        }

        private string GetTechnicalStatusCode(TechnicalStatus status)
        {
            return status switch
            {
                TechnicalStatus.Good => "good",
                TechnicalStatus.Fair => "fair",
                TechnicalStatus.NeedsCheck => "needs-check",
                TechnicalStatus.NeedsRepair => "needs-repair",
                _ => "unknown"
            };
        }

        private string GetDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            if (field == null) return value.ToString();

            var attribute = (DescriptionAttribute?)Attribute.GetCustomAttribute(
                field, typeof(DescriptionAttribute));
            return attribute?.Description ?? value.ToString();
        }

        private VehicleStatusResponse BuildVehicleStatusResponse(Vehicle vehicle)
        {
            return new VehicleStatusResponse
            {
                VehicleId = vehicle.VehicleId,
                VehicleCode = $"VE-{vehicle.VehicleId:D3}", // Format: VE-001, VE-002...
                VehicleName = vehicle.Model?.ModelName ?? $"Vehicle {vehicle.VehicleId}",
                Model = vehicle.Model?.ModelName ?? "N/A",
                Status = vehicle.Status.ToLower(),
                BatteryLevel = vehicle.CurrentBatteryLevel,
                TechnicalStatus = GetTechnicalStatusCode(vehicle.CurrentTechnicalStatus),
                TechnicalStatusDescription = GetDescription(vehicle.CurrentTechnicalStatus),
                NextBooking = null, // TODO: Implement if you have Booking model
                LastUpdate = vehicle.LastStatusUpdate?.ToString("HH:mm - dd/MM/yyyy") ?? "Chưa cập nhật",
                LastUpdatedBy = vehicle.LastUpdatedBy.HasValue ? $"User #{vehicle.LastUpdatedBy}" : "N/A"
            };
        }
    }


}
