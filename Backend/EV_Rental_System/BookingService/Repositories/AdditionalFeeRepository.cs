using BookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Repositories
{
    public class AdditionalFeeRepository : IAdditionalFeeRepository
    {
        private readonly MyDbContext _context;

        public AdditionalFeeRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<AdditionalFee?> GetByIdAsync(int feeId)
        {
            return await _context.AdditionalFees
                .Include(f => f.Order)
                .FirstOrDefaultAsync(f => f.FeeId == feeId);
        }

        public async Task<List<AdditionalFee>> GetByOrderIdAsync(int orderId)
        {
            return await _context.AdditionalFees
                .Where(f => f.OrderId == orderId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<AdditionalFee> AddAsync(AdditionalFee fee)
        {
            _context.AdditionalFees.Add(fee);
            await _context.SaveChangesAsync();
            return fee;
        }

        public async Task<bool> UpdateAsync(AdditionalFee fee)
        {
            _context.AdditionalFees.Update(fee);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int feeId)
        {
            var fee = await _context.AdditionalFees.FindAsync(feeId);
            if (fee == null) return false;

            _context.AdditionalFees.Remove(fee);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<AdditionalFee>> GetUnpaidFeesByOrderIdAsync(int orderId)
        {
            return await _context.AdditionalFees
                .Where(f => f.OrderId == orderId && !f.IsPaid)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalFeesByOrderIdAsync(int orderId)
        {
            return await _context.AdditionalFees
                .Where(f => f.OrderId == orderId)
                .SumAsync(f => f.Amount);
        }

        public async Task<bool> MarkAsPaidAsync(int feeId)
        {
            var fee = await _context.AdditionalFees.FindAsync(feeId);
            if (fee == null) return false;

            fee.IsPaid = true;
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
