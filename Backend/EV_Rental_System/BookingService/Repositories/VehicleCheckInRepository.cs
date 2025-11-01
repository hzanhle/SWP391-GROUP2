using BookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Repositories
{
    public class VehicleCheckInRepository : IVehicleCheckInRepository
    {
        private readonly MyDbContext _context;

        public VehicleCheckInRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<VehicleCheckIn?> GetByIdAsync(int checkInId)
        {
            return await _context.VehicleCheckIns
                .Include(c => c.Order)
                .FirstOrDefaultAsync(c => c.CheckInId == checkInId);
        }

        public async Task<VehicleCheckIn?> GetByOrderIdAsync(int orderId)
        {
            return await _context.VehicleCheckIns
                .Include(c => c.Order)
                .FirstOrDefaultAsync(c => c.OrderId == orderId);
        }

        public async Task<VehicleCheckIn> CreateAsync(VehicleCheckIn checkIn)
        {
            checkIn.CreatedAt = DateTime.UtcNow;
            _context.VehicleCheckIns.Add(checkIn);
            await _context.SaveChangesAsync();
            return checkIn;
        }

        public async Task<VehicleCheckIn> UpdateAsync(VehicleCheckIn checkIn)
        {
            _context.VehicleCheckIns.Update(checkIn);
            await _context.SaveChangesAsync();
            return checkIn;
        }

        public async Task<bool> DeleteAsync(int checkInId)
        {
            var checkIn = await _context.VehicleCheckIns.FindAsync(checkInId);
            if (checkIn == null) return false;

            _context.VehicleCheckIns.Remove(checkIn);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<VehicleCheckIn>> GetAllAsync()
        {
            return await _context.VehicleCheckIns
                .Include(c => c.Order)
                .ToListAsync();
        }
    }
}
