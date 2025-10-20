using BookingSerivce.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingSerivce.Repositories
{
    /// <summary>
    /// Repository implementation for SoftLock entity.
    /// Handles database operations for temporary vehicle reservation locks.
    /// </summary>
    public class SoftLockRepository : ISoftLockRepository
    {
        private readonly MyDbContext _context;

        public SoftLockRepository(MyDbContext context)
        {
            _context = context;
        }

        // Basic CRUD operations
        public async Task<SoftLock?> GetByTokenAsync(Guid lockToken)
        {
            return await _context.SoftLocks
                .FirstOrDefaultAsync(sl => sl.LockToken == lockToken);
        }

        public async Task<IEnumerable<SoftLock>> GetAllAsync()
        {
            return await _context.SoftLocks.ToListAsync();
        }

        public async Task AddAsync(SoftLock softLock)
        {
            await _context.SoftLocks.AddAsync(softLock);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(SoftLock softLock)
        {
            _context.SoftLocks.Update(softLock);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid lockToken)
        {
            var softLock = await GetByTokenAsync(lockToken);
            if (softLock != null)
            {
                _context.SoftLocks.Remove(softLock);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid lockToken)
        {
            return await _context.SoftLocks.AnyAsync(sl => sl.LockToken == lockToken);
        }

        // Query operations
        public async Task<IEnumerable<SoftLock>> GetByUserIdAsync(int userId)
        {
            return await _context.SoftLocks
                .Where(sl => sl.UserId == userId)
                .OrderByDescending(sl => sl.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<SoftLock>> GetByVehicleIdAsync(int vehicleId)
        {
            return await _context.SoftLocks
                .Where(sl => sl.VehicleId == vehicleId)
                .OrderByDescending(sl => sl.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<SoftLock>> GetByStatusAsync(string status)
        {
            return await _context.SoftLocks
                .Where(sl => sl.Status == status)
                .ToListAsync();
        }

        public async Task<IEnumerable<SoftLock>> GetActiveLocksAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.SoftLocks
                .Where(sl => sl.Status == "Active" && sl.ExpiresAt > now)
                .ToListAsync();
        }

        public async Task<IEnumerable<SoftLock>> GetExpiredLocksAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.SoftLocks
                .Where(sl => sl.Status == "Active" && sl.ExpiresAt <= now)
                .ToListAsync();
        }

        // Business logic operations
        public async Task<IEnumerable<SoftLock>> GetVehicleActiveLocksAsync(int vehicleId, DateTime fromDate, DateTime toDate)
        {
            var now = DateTime.UtcNow;
            return await _context.SoftLocks
                .Where(sl => sl.VehicleId == vehicleId &&
                            sl.Status == "Active" &&
                            sl.ExpiresAt > now &&
                            sl.FromDate < toDate &&
                            sl.ToDate > fromDate)
                .ToListAsync();
        }

        public async Task<bool> HasActiveLockAsync(int vehicleId, DateTime fromDate, DateTime toDate)
        {
            var activeLocks = await GetVehicleActiveLocksAsync(vehicleId, fromDate, toDate);
            return activeLocks.Any();
        }

        public async Task CleanupExpiredLocksAsync()
        {
            var expiredLocks = await GetExpiredLocksAsync();
            foreach (var softLock in expiredLocks)
            {
                softLock.Expire();
            }
            await _context.SaveChangesAsync();
        }

        public async Task<int> CountActiveLocksForVehicleAsync(int vehicleId)
        {
            var now = DateTime.UtcNow;
            return await _context.SoftLocks
                .CountAsync(sl => sl.VehicleId == vehicleId &&
                                 sl.Status == "Active" &&
                                 sl.ExpiresAt > now);
        }
    }
}
