using Microsoft.EntityFrameworkCore;
using TwoWheelVehicleService.DTOs;
using TwoWheelVehicleService.Models;

namespace TwoWheelVehicleService.Repositories
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly MyDbContext _context;

        public VehicleRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<VehicleDTO?> GetAvailableVehicleAsync(int modelId, string color, int stationId)
        {
            return await _context.Vehicles
                .Where(v => v.ModelId == modelId
                         && v.Color == color
                         && v.StationId == stationId
                         && v.IsActive == true
                         && v.Status == "Available")
                .Select(v => new VehicleDTO
                {
                    VehicleId = v.VehicleId,
                    StationId = v.StationId,
                    ModelId = v.ModelId,
                    LicensePlate = v.LicensePlate,
                    Color = v.Color,
                    IsActive = v.IsActive,
                    Status = v.Status
                })
                .FirstOrDefaultAsync();
        }
        public async Task<List<Vehicle>> GetVehiclesByStatus(string status) // Get Vehicles by Status
        {
            return await _context.Vehicles.Where(v => v.Status == status).ToListAsync();
        }

        public async Task<List<Vehicle>> GetActiveVehicles() // Get Active Vehicles
        {
            return await _context.Vehicles.Where(v => v.IsActive).ToListAsync();
        }
        public async Task<List<Vehicle>> GetAllVehicles() // Get All Vehicles
        {
            return await _context.Vehicles
                        .Include(v => v.Model) //include để lấy model name
                        .ToListAsync();
        }
        public async Task AddVehicle(Vehicle vehicle)
        {
            await _context.Vehicles.AddAsync(vehicle);
            await _context.SaveChangesAsync();
        }

        public async Task ChangeStatus(int vehicleId, string status)
        {
            var vehicle = await _context.Vehicles.FindAsync(vehicleId);
            if (vehicle != null)
            {
                vehicle.Status = status;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Vehicle?> GetVehicleById(int Id)
        {
            return await _context.Vehicles
                .Include(v => v.Model)
                .FirstOrDefaultAsync(v => v.VehicleId == Id);
        }

        public async Task UpdateVehicle(Vehicle vehicle)
        {
            _context.Vehicles.Update(vehicle);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteVehicle(int vehicleId)
        {
            var vehicle = _context.Vehicles.Find(vehicleId);
            if (vehicle != null)
            {
                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();
            }
        }

        public Task<List<Vehicle>> GetAllVehiclesByModelId(int modelId)
        {
            var vehicles = _context.Vehicles
                .Include(v => v.Model)
                .Where(v => v.ModelId == modelId)
                .ToListAsync();
            return vehicles;
        }

        public async Task<List<VehicleStatusHistory>> GetVehicleHistoryAsync(int vehicleId)
        {
            return await _context.VehicleStatusHistories
                .Where(h => h.VehicleId == vehicleId)
                .OrderByDescending(h => h.UpdatedAt)
                .ToListAsync();
        }

        public async Task<VehicleStatusHistory?> GetLatestStatusAsync(int vehicleId)
        {
            return await _context.VehicleStatusHistories
                .Where(h => h.VehicleId == vehicleId)
                .OrderByDescending(h => h.UpdatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<VehicleStatusHistory> CreateHistoryAsync(VehicleStatusHistory history)
        {
            _context.VehicleStatusHistories.Add(history);
            await _context.SaveChangesAsync();
            return history;
        }

        public async Task<List<Vehicle>> GetVehiclesWithFiltersAsync(
            string? statusFilter,
            string? batteryFilter,
            string? search,
            int? stationId)
        {
            var query = _context.Vehicles
                .Include(v => v.Model) //include model để lấy modelname
                .AsQueryable();

            // Filter by station (nếu có StationId)
            if (stationId.HasValue)
                query = query.Where(v => v.StationId == stationId.Value);

            // Filter by status
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "all")
            {
                query = query.Where(v => v.Status.ToLower() == statusFilter.ToLower());
            }

            // Filter by battery
            if (!string.IsNullOrEmpty(batteryFilter) && batteryFilter != "all")
            {
                query = batteryFilter.ToLower() switch
                {
                    "low" => query.Where(v => v.CurrentBatteryLevel < 30),
                    "medium" => query.Where(v => v.CurrentBatteryLevel >= 30 && v.CurrentBatteryLevel < 70),
                    "high" => query.Where(v => v.CurrentBatteryLevel >= 70),
                    _ => query
                };
            }

            // Filter by search - FIXED: Tìm theo VehicleId hoặc ModelName
            if (!string.IsNullOrEmpty(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(v =>
                    v.VehicleId.ToString().Contains(search) || // Tìm theo VehicleId
                    (v.Model != null && v.Model.ModelName.ToLower().Contains(lowerSearch)) || // Tìm theo ModelName
                    v.Color.ToLower().Contains(lowerSearch)); // Tìm theo Color
            }

            return await query.ToListAsync();
        }
    }
}

