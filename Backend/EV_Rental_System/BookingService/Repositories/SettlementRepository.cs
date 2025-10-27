using BookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Repositories
{
    public class SettlementRepository : ISettlementRepository
    {
        private readonly MyDbContext _context;

        public SettlementRepository(MyDbContext context)
        {
            _context = context;
        }

        // === CREATE ===
        public async Task<Settlement> CreateAsync(Settlement settlement)
        {
            await _context.Settlements.AddAsync(settlement);
            await _context.SaveChangesAsync();
            return settlement;
        }

        // === READ ===
        public async Task<Settlement?> GetByIdAsync(int settlementId)
        {
            return await _context.Settlements
                .Include(s => s.Order)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SettlementId == settlementId);
        }

        public async Task<Settlement?> GetByOrderIdAsync(int orderId)
        {
            return await _context.Settlements
                .Include(s => s.Order)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.OrderId == orderId);
        }

        public async Task<IEnumerable<Settlement>> GetAllAsync()
        {
            return await _context.Settlements
                .Include(s => s.Order)
                .AsNoTracking()
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        // === UPDATE ===
        public async Task<bool> UpdateAsync(Settlement settlement)
        {
            _context.Settlements.Update(settlement);
            return await _context.SaveChangesAsync() > 0;
        }

        // === DELETE ===
        public async Task<bool> DeleteAsync(int settlementId)
        {
            var settlement = await _context.Settlements.FindAsync(settlementId);
            if (settlement == null)
                return false;

            _context.Settlements.Remove(settlement);
            return await _context.SaveChangesAsync() > 0;
        }

        // === FILTER QUERIES ===
        public async Task<IEnumerable<Settlement>> GetFinalizedSettlementsAsync()
        {
            return await _context.Settlements
                .Include(s => s.Order)
                .AsNoTracking()
                .Where(s => s.IsFinalized)
                .OrderByDescending(s => s.FinalizedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Settlement>> GetPendingSettlementsAsync()
        {
            return await _context.Settlements
                .Include(s => s.Order)
                .AsNoTracking()
                .Where(s => !s.IsFinalized)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> ExistsByOrderIdAsync(int orderId)
        {
            return await _context.Settlements
                .AnyAsync(s => s.OrderId == orderId);
        }
    }
}
