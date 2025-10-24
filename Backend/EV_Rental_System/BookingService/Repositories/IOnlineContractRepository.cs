using BookingService.Models;

namespace BookingService.Repositories
{
    public interface IOnlineContractRepository
    {
        Task<OnlineContract> CreateAsync(OnlineContract contract);
        Task<bool> UpdateAsync(OnlineContract contract);
        Task<OnlineContract?> GetByIdAsync(int contractId);
        Task<OnlineContract?> GetByOrderIdAsync(int orderId);
        Task<OnlineContract?> GetByContractNumberAsync(string contractNumber);
        Task<bool> ExistsByOrderIdAsync(int orderId);

        // ✅ THÊM: Kiểm tra contract number có tồn tại không
        Task<bool> ExistsByContractNumberAsync(string contractNumber);

        // ✅ THÊM: Lấy contract number lớn nhất theo pattern (để generate số tiếp theo)
        Task<string?> GetLatestContractNumberByDateAsync(string datePrefix);
    }
}