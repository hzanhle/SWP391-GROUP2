using BookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Repositories
{
    public class OnlineContractRepository : IOnlineContractRepository
    {
        private readonly MyDbContext _context;

        public OnlineContractRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<OnlineContract> CreateAsync(OnlineContract contract)
        {
            await _context.OnlineContracts.AddAsync(contract);
            await _context.SaveChangesAsync();
            return contract;
        }

        public async Task<bool> UpdateAsync(OnlineContract contract)
        {
            _context.OnlineContracts.Update(contract);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<OnlineContract?> GetByIdAsync(int contractId)
        {
            return await _context.OnlineContracts
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.OnlineContractId == contractId);
        }

        public async Task<OnlineContract?> GetByOrderIdAsync(int orderId)
        {
            return await _context.OnlineContracts
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.OrderId == orderId);
        }

        public async Task<OnlineContract?> GetByContractNumberAsync(string contractNumber)
        {
            return await _context.OnlineContracts
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ContractNumber == contractNumber);
        }

        public async Task<bool> ExistsByOrderIdAsync(int orderId)
        {
            return await _context.OnlineContracts
                .AsNoTracking()
                .AnyAsync(c => c.OrderId == orderId);
        }

        // ✅ THÊM: Kiểm tra contract number có tồn tại không
        public async Task<bool> ExistsByContractNumberAsync(string contractNumber)
        {
            return await _context.OnlineContracts
                .AsNoTracking()
                .AnyAsync(c => c.ContractNumber == contractNumber);
        }

        // ✅ THÊM: Lấy contract number lớn nhất theo ngày
        // Ví dụ: datePrefix = "CT-20251023-" -> Trả về "CT-20251023-000005"
        public async Task<string?> GetLatestContractNumberByDateAsync(string datePrefix)
        {
            return await _context.OnlineContracts
                .AsNoTracking()
                .Where(c => c.ContractNumber.StartsWith(datePrefix))
                .OrderByDescending(c => c.ContractNumber)
                .Select(c => c.ContractNumber)
                .FirstOrDefaultAsync();
        }
    }
}