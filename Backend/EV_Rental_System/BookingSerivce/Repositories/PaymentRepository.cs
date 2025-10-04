using BookingService.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookingSerivce.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly MyDbContext _context;

        public PaymentRepository(MyDbContext context)
        {
            _context = context;
        }

        // ===== BASIC CRUD =====
        public async Task<Payment?> GetByIdAsync(int paymentId)
        {
            return await _context.Payments.FindAsync(paymentId);
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _context.Payments.ToListAsync();
        }

        public async Task<Payment> AddAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task UpdateAsync(Payment payment)
        {
            payment.UpdatedAt = DateTime.UtcNow;
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int paymentId)
        {
            var payment = await GetByIdAsync(paymentId);
            if (payment != null)
            {
                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int paymentId)
        {
            return await _context.Payments.AnyAsync(p => p.PaymentId == paymentId);
        }

        // ===== QUERY METHODS =====
        public async Task<Payment?> GetByOrderIdAsync(int orderId)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.OrderId == orderId);
        }

        public async Task<Payment?> GetByTransactionCodeAsync(string transactionCode)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.TransactionCode == transactionCode);
        }

        public async Task<Payment?> GetByDepositTransactionCodeAsync(string depositTransactionCode)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.DepositTransactionCode == depositTransactionCode);
        }

        public async Task<IEnumerable<Payment>> GetByStatusAsync(string status)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .Where(p => p.Status == status)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetByPaymentMethodAsync(string paymentMethod)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .Where(p => p.PaymentMethod == paymentMethod)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync()
        {
            return await _context.Payments
                .Include(p => p.Order)
                .Where(p => p.Status == "Pending" || p.Status == "PendingDeposit" || p.Status == "PendingFullPayment")
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetDepositedPaymentsAsync()
        {
            return await _context.Payments
                .Include(p => p.Order)
                .Where(p => p.IsDeposited && !p.IsFullyPaid)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetFullyPaidPaymentsAsync()
        {
            return await _context.Payments
                .Include(p => p.Order)
                .Where(p => p.IsFullyPaid)
                .ToListAsync();
        }

        // ===== ADVANCED QUERIES =====
        public async Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsWithOrdersAsync()
        {
            return await _context.Payments
                .Include(p => p.Order)
                .ToListAsync();
        }

        public async Task<Payment?> GetPaymentWithOrderByIdAsync(int paymentId)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);
        }

        public async Task<IEnumerable<Payment>> FindAsync(Expression<Func<Payment, bool>> predicate)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .Where(predicate)
                .ToListAsync();
        }

        // ===== STATISTICS =====
        public async Task<decimal> GetTotalPaidAmountAsync()
        {
            return await _context.Payments
                .Where(p => p.IsFullyPaid)
                .SumAsync(p => p.PaidAmount);
        }

        public async Task<decimal> GetTotalPaidAmountByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Payments
                .Where(p => p.FullPaymentDate >= startDate && p.FullPaymentDate <= endDate)
                .SumAsync(p => p.PaidAmount);
        }

        public async Task<int> GetPaymentCountByStatusAsync(string status)
        {
            return await _context.Payments
                .CountAsync(p => p.Status == status);
        }

        public async Task<Dictionary<string, int>> GetPaymentCountByMethodAsync()
        {
            return await _context.Payments
                .GroupBy(p => p.PaymentMethod)
                .Select(g => new { Method = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Method, x => x.Count);
        }

        // ===== BUSINESS LOGIC HELPERS =====
        public async Task<bool> HasDepositedAsync(int orderId)
        {
            var payment = await GetByOrderIdAsync(orderId);
            return payment?.IsDeposited ?? false;
        }

        public async Task<bool> IsFullyPaidAsync(int orderId)
        {
            var payment = await GetByOrderIdAsync(orderId);
            return payment?.IsFullyPaid ?? false;
        }

        public async Task<decimal> GetTotalPaidForOrderAsync(int orderId)
        {
            var payment = await GetByOrderIdAsync(orderId);
            return payment?.PaidAmount ?? 0;
        }

        public async Task<decimal> GetDepositedAmountForOrderAsync(int orderId)
        {
            var payment = await GetByOrderIdAsync(orderId);
            return payment?.DepositedAmount ?? 0;
        }
    }
}

// 3. Program.cs - Register repository
/*

*/

// 4. Cách sử dụng trong PaymentService

