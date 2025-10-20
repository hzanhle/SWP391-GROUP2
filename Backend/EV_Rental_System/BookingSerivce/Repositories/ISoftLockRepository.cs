using BookingSerivce.Models;

namespace BookingSerivce.Repositories
{
    /// <summary>
    /// Repository interface for SoftLock entity operations.
    /// Manages temporary vehicle reservations during the preview-to-confirm window.
    /// </summary>
    public interface ISoftLockRepository
    {
        // Basic CRUD operations
        Task<SoftLock?> GetByTokenAsync(Guid lockToken);
        Task<IEnumerable<SoftLock>> GetAllAsync();
        Task AddAsync(SoftLock softLock);
        Task UpdateAsync(SoftLock softLock);
        Task DeleteAsync(Guid lockToken);
        Task<bool> ExistsAsync(Guid lockToken);

        // Query operations
        Task<IEnumerable<SoftLock>> GetByUserIdAsync(int userId);
        Task<IEnumerable<SoftLock>> GetByVehicleIdAsync(int vehicleId);
        Task<IEnumerable<SoftLock>> GetByStatusAsync(string status);
        Task<IEnumerable<SoftLock>> GetActiveLocksAsync();
        Task<IEnumerable<SoftLock>> GetExpiredLocksAsync();

        // Business logic operations
        Task<IEnumerable<SoftLock>> GetVehicleActiveLocksAsync(int vehicleId, DateTime fromDate, DateTime toDate);
        Task<bool> HasActiveLockAsync(int vehicleId, DateTime fromDate, DateTime toDate);
        Task CleanupExpiredLocksAsync();
        Task<int> CountActiveLocksForVehicleAsync(int vehicleId);
    }
}
