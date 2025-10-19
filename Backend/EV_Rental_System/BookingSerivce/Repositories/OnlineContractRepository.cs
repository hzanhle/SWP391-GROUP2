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
            // Đảm bảo EF Core theo dõi (track) sự thay đổi của entity
            _context.OnlineContracts.Update(contract);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<OnlineContract?> GetByIdAsync(int contractId)
        {
            return await _context.OnlineContracts
                .AsNoTracking() // Dùng AsNoTracking cho các truy vấn chỉ đọc
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
    }
}