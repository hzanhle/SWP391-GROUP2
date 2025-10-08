using BookingSerivce;
using BookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingSerivce.Repositories
{
    public class ContractRepository : IContractRepository
    {
        private readonly MyDbContext _context;

        public ContractRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<OnlineContract> AddAsync(OnlineContract contract)
        {
            _context.OnlineContracts.Add(contract);
            await _context.SaveChangesAsync();
            return contract;
        }

        public async Task<OnlineContract?> GetByIdAsync(int contractId)
        {
            return await _context.OnlineContracts
                .Include(c => c.Order)
                .FirstOrDefaultAsync(c => c.ContractId == contractId);
        }

        public async Task<OnlineContract?> GetByOrderIdAsync(int orderId)
        {
            return await _context.OnlineContracts
                .Include(c => c.Order)
                .FirstOrDefaultAsync(c => c.OrderId == orderId);
        }

        public async Task<OnlineContract> UpdateAsync(OnlineContract contract)
        {
            contract.UpdatedAt = DateTime.UtcNow;
            _context.OnlineContracts.Update(contract);
            await _context.SaveChangesAsync();
            return contract;
        }

        public async Task<IEnumerable<OnlineContract>> GetByUserIdAsync(int userId)
        {
            return await _context.OnlineContracts
                .Include(c => c.Order)
                .Where(c => c.Order != null && c.Order.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<string> GenerateContractNumberAsync()
        {
            // Format: CT-YYYY-XXXXX
            var year = DateTime.UtcNow.Year;
            var prefix = $"CT-{year}-";

            // Get the latest contract number for this year
            var latestContract = await _context.OnlineContracts
                .Where(c => c.ContractNumber.StartsWith(prefix))
                .OrderByDescending(c => c.ContractNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (latestContract != null)
            {
                var lastNumberStr = latestContract.ContractNumber.Substring(prefix.Length);
                if (int.TryParse(lastNumberStr, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D5}";
        }
    }
}
