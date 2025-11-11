using BookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Repositories
{
    public class VehicleReturnRepository : IVehicleReturnRepository
    {
        private readonly MyDbContext _context;

        public VehicleReturnRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<VehicleReturn?> GetByIdAsync(int returnId)
        {
            return await _context.VehicleReturns
                .Include(r => r.Order)
                .FirstOrDefaultAsync(r => r.ReturnId == returnId);
        }

        public async Task<VehicleReturn?> GetByOrderIdAsync(int orderId)
        {
            return await _context.VehicleReturns
                .Include(r => r.Order)
                .FirstOrDefaultAsync(r => r.OrderId == orderId);
        }

        public async Task<VehicleReturn> CreateAsync(VehicleReturn vehicleReturn)
        {
            vehicleReturn.CreatedAt = DateTime.UtcNow;
            _context.VehicleReturns.Add(vehicleReturn);
            await _context.SaveChangesAsync();
            return vehicleReturn;
        }

        public async Task<VehicleReturn> UpdateAsync(VehicleReturn vehicleReturn)
        {
            _context.VehicleReturns.Update(vehicleReturn);
            await _context.SaveChangesAsync();
            return vehicleReturn;
        }

        public async Task<bool> DeleteAsync(int returnId)
        {
            var vehicleReturn = await _context.VehicleReturns.FindAsync(returnId);
            if (vehicleReturn == null) return false;

            _context.VehicleReturns.Remove(vehicleReturn);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<VehicleReturn>> GetAllAsync()
        {
            return await _context.VehicleReturns
                .Include(r => r.Order)
                .ToListAsync();
        }
    }
}
