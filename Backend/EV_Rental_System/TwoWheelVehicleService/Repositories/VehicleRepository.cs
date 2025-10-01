using Microsoft.EntityFrameworkCore;
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
            return await _context.Vehicles.ToListAsync();
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

        public async Task ChangeStatus(int vehicleId)
        {
            var vehicle = await _context.Models.FindAsync(vehicleId);
            if (vehicle != null && vehicle.IsActive == true)
            {
                vehicle.IsActive = false; // Soft delete by setting IsActive to false
                await _context.SaveChangesAsync();
            }
            else
            {
                vehicle.IsActive = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Vehicle> GetVehicleById(int Id)
        {
            return await _context.Vehicles.FindAsync(Id);
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
    }
}
