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

        public async Task<int> CreateAsync(OnlineContract contract)
        {
            _context.OnlineContracts.Add(contract);
            await _context.SaveChangesAsync();
            return contract.OnlineContractId;
        }

        public async Task<OnlineContract?> GetByIdAsync(int contractId)
        {
            return await _context.OnlineContracts
                .Include(c => c.Order)
                .FirstOrDefaultAsync(c => c.OnlineContractId == contractId);
        }

        public async Task<OnlineContract?> GetByOrderIdAsync(int orderId)
        {
            return await _context.OnlineContracts
                .Include(c => c.Order)
                .FirstOrDefaultAsync(c => c.OrderId == orderId);
        }

        public async Task<OnlineContract?> GetByContractNumberAsync(string contractNumber)
        {
            return await _context.OnlineContracts
                .Include(c => c.Order)
                .FirstOrDefaultAsync(c => c.ContractNumber == contractNumber);
        }

        public async Task<IEnumerable<OnlineContract>> GetByStatusAsync(string status)
        {
            return await _context.OnlineContracts
                .Include(c => c.Order)
                .Where(c => c.Status == status)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<OnlineContract>> GetExpiredContractsAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.OnlineContracts
                .Include(c => c.Order)
                .Where(c => c.Status != "Signed" &&
                           c.ExpiresAt.HasValue &&
                           c.ExpiresAt.Value < now)
                .ToListAsync();
        }

        public async Task<IEnumerable<OnlineContract>> GetExpiringContractsAsync(int hoursThreshold)
        {
            var now = DateTime.UtcNow;
            var threshold = now.AddHours(hoursThreshold);

            return await _context.OnlineContracts
                .Include(c => c.Order)
                .Where(c => c.Status == "Draft" &&
                           c.ExpiresAt.HasValue &&
                           c.ExpiresAt.Value >= now &&
                           c.ExpiresAt.Value <= threshold)
                .OrderBy(c => c.ExpiresAt)
                .ToListAsync();
        }

        public async Task<bool> UpdateAsync(OnlineContract contract)
        {
            var existingContract = await _context.OnlineContracts.FindAsync(contract.OnlineContractId);
            if (existingContract == null) return false;

            existingContract.ContractNumber = contract.ContractNumber;
            existingContract.ContractFilePath = contract.ContractFilePath;
            existingContract.Status = contract.Status;
            existingContract.SignedAt = contract.SignedAt;
            existingContract.SignatureData = contract.SignatureData;
            existingContract.ExpiresAt = contract.ExpiresAt;
            existingContract.TemplateVersion = contract.TemplateVersion;
            existingContract.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int contractId)
        {
            var contract = await _context.OnlineContracts.FindAsync(contractId);
            if (contract == null) return false;

            _context.OnlineContracts.Remove(contract);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsByOrderIdAsync(int orderId)
        {
            return await _context.OnlineContracts.AnyAsync(c => c.OrderId == orderId);
        }

        public async Task<int> CountByStatusAsync(string status)
        {
            return await _context.OnlineContracts
                .Where(c => c.Status == status)
                .CountAsync();
        }
    }
}
