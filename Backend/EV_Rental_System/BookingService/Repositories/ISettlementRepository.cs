using BookingService.Models;

namespace BookingService.Repositories
{
    public interface ISettlementRepository
    {
        // === CRUD ===
        Task<Settlement> CreateAsync(Settlement settlement);
        Task<Settlement?> GetByIdAsync(int settlementId);
        Task<Settlement?> GetByOrderIdAsync(int orderId);
        Task<bool> UpdateAsync(Settlement settlement);
        Task<bool> DeleteAsync(int settlementId);

        // === QUERIES ===
        Task<IEnumerable<Settlement>> GetAllAsync();
        Task<IEnumerable<Settlement>> GetFinalizedSettlementsAsync();
        Task<IEnumerable<Settlement>> GetPendingSettlementsAsync(); // Not finalized yet
        Task<bool> ExistsByOrderIdAsync(int orderId);
    }
}
