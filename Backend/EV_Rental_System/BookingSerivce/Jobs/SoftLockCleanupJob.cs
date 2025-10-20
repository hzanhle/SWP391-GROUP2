using BookingSerivce.Repositories;

namespace BookingSerivce.Jobs
{
    /// <summary>
    /// Background job that cleans up expired soft locks.
    /// Runs every 1 minute to mark expired locks as "Expired".
    /// </summary>
    public class SoftLockCleanupJob
    {
        private readonly ISoftLockRepository _softLockRepository;
        private readonly ILogger<SoftLockCleanupJob> _logger;

        public SoftLockCleanupJob(
            ISoftLockRepository softLockRepository,
            ILogger<SoftLockCleanupJob> logger)
        {
            _softLockRepository = softLockRepository;
            _logger = logger;
        }

        /// <summary>
        /// Cleans up expired soft locks.
        /// Called by Hangfire every 1 minute.
        /// </summary>
        public async Task CleanupExpiredLocksAsync()
        {
            try
            {
                var expiredLocks = await _softLockRepository.GetExpiredLocksAsync();
                var expiredList = expiredLocks.ToList();

                if (!expiredList.Any())
                {
                    _logger.LogDebug("No expired soft locks found");
                    return;
                }

                _logger.LogInformation($"Cleaning up {expiredList.Count} expired soft locks");

                foreach (var softLock in expiredList)
                {
                    try
                    {
                        softLock.Expire();
                        await _softLockRepository.UpdateAsync(softLock);

                        _logger.LogDebug($"SoftLock {softLock.LockToken} expired (Vehicle: {softLock.VehicleId})");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error expiring soft lock {softLock.LockToken}");
                    }
                }

                _logger.LogInformation($"Successfully cleaned up {expiredList.Count} soft locks");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SoftLockCleanupJob");
                throw;
            }
        }
    }
}
