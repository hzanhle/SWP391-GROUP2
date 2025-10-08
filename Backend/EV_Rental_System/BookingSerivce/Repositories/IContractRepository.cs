using BookingService.Models;

namespace BookingSerivce.Repositories
{
    public interface IContractRepository
    {
        Task<OnlineContract> AddAsync(OnlineContract contract);
        Task<OnlineContract?> GetByIdAsync(int contractId);
        Task<OnlineContract?> GetByOrderIdAsync(int orderId);
        Task<OnlineContract> UpdateAsync(OnlineContract contract);
        Task<IEnumerable<OnlineContract>> GetByUserIdAsync(int userId);
        Task<string> GenerateContractNumberAsync();
    }
}
