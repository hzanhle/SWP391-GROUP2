using BookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly MyDbContext _context;

        public PaymentRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<int> CreateAsync(Payment payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment.PaymentId;
        }

        public async Task<Payment?> GetByIdAsync(int paymentId)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);
        }

        public async Task<Payment?> GetByOrderIdAsync(int orderId)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.OrderId == orderId);
        }

        public async Task<Payment?> GetByTransactionIdAsync(string transactionId)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.TransactionId == transactionId);
        }

        public async Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .Where(p => p.Status == status)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetByPaymentMethodAsync(string paymentMethod)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .Where(p => p.PaymentMethod == paymentMethod)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync()
        {
            return await _context.Payments
                .Include(p => p.Order)
                .Where(p => p.Status == PaymentStatus.Pending)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> UpdateAsync(Payment payment)
        {
            var existingPayment = await _context.Payments.FindAsync(payment.PaymentId);
            if (existingPayment == null) return false;

            existingPayment.Amount = payment.Amount;
            existingPayment.PaymentMethod = payment.PaymentMethod;
            existingPayment.Status = payment.Status;
            existingPayment.TransactionId = payment.TransactionId;
            existingPayment.PaidAt = payment.PaidAt;
            existingPayment.PaymentGatewayResponse = payment.PaymentGatewayResponse;
            existingPayment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int paymentId)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null) return false;

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsByOrderIdAsync(int orderId)
        {
            return await _context.Payments.AnyAsync(p => p.OrderId == orderId);
        }
    }
}
