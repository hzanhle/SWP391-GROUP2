using BookingSerivce.Models;
using BookingService.Models;
using System.Linq.Expressions;

namespace BookingSerivce.Repositories
{
    public interface IPaymentRepository
    {
        // Basic CRUD
        Task<Payment?> GetByIdAsync(int paymentId);
        Task<IEnumerable<Payment>> GetAllAsync();
        Task<Payment> AddAsync(Payment payment);
        Task UpdateAsync(Payment payment);
        Task DeleteAsync(int paymentId);
        Task<bool> ExistsAsync(int paymentId);

        // Query methods
        Task<Payment?> GetByOrderIdAsync(int orderId);
        Task<Payment?> GetByTransactionCodeAsync(string transactionCode);
        Task<Payment?> GetByDepositTransactionCodeAsync(string depositTransactionCode);
        Task<IEnumerable<Payment>> GetByStatusAsync(string status);
        Task<IEnumerable<Payment>> GetByPaymentMethodAsync(string paymentMethod);
        Task<IEnumerable<Payment>> GetPendingPaymentsAsync();
        Task<IEnumerable<Payment>> GetDepositedPaymentsAsync();
        Task<IEnumerable<Payment>> GetFullyPaidPaymentsAsync();

        // Advanced queries
        Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Payment>> GetPaymentsWithOrdersAsync();
        Task<Payment?> GetPaymentWithOrderByIdAsync(int paymentId);
        Task<IEnumerable<Payment>> FindAsync(Expression<Func<Payment, bool>> predicate);

        // Statistics
        Task<decimal> GetTotalPaidAmountAsync();
        Task<decimal> GetTotalPaidAmountByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<int> GetPaymentCountByStatusAsync(string status);
        Task<Dictionary<string, int>> GetPaymentCountByMethodAsync();

        // Business logic helpers
        Task<bool> HasDepositedAsync(int orderId);
        Task<bool> IsFullyPaidAsync(int orderId);
        Task<decimal> GetTotalPaidForOrderAsync(int orderId);
        Task<decimal> GetDepositedAmountForOrderAsync(int orderId);
    }
}
