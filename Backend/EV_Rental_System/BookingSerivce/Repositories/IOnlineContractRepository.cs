using BookingService.Models;

namespace BookingService.Repositories
{
    public interface IOnlineContractRepository
    {
        Task<int> CreateAsync(OnlineContract contract);
        Task<OnlineContract?> GetByIdAsync(int contractId);
        Task<OnlineContract?> GetByOrderIdAsync(int orderId);
        Task<OnlineContract?> GetByContractNumberAsync(string contractNumber);
        Task<IEnumerable<OnlineContract>> GetByStatusAsync(string status);
        Task<IEnumerable<OnlineContract>> GetExpiredContractsAsync();
        Task<IEnumerable<OnlineContract>> GetExpiringContractsAsync(int hoursThreshold);
        Task<bool> ExistsByOrderIdAsync(int orderId);
        Task<int> CountByStatusAsync(string status);
        
    }
}
