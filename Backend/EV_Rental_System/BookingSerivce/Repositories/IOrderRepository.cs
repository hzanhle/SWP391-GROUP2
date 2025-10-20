using BookingSerivce.Models;
using BookingService.Models;
using System.Linq.Expressions;

namespace BookingSerivce.Repositories
{
    public interface IOrderRepository
    {
        // Basic CRUD
        Task<Order?> GetByIdAsync(int orderId);
        Task<IEnumerable<Order>> GetAllAsync();
        Task<Order> AddAsync(Order order);
        Task<Order> UpdateAsync(Order order);
        Task DeleteAsync(int orderId);
        Task<bool> ExistsAsync(int orderId);

        // Query methods
        Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Order>> GetByVehicleIdAsync(int vehicleId);
        Task<IEnumerable<Order>> GetByStatusAsync(string status);
        Task<IEnumerable<Order>> GetPendingOrdersAsync();
        Task<IEnumerable<Order>> GetConfirmedOrdersAsync();
        Task<IEnumerable<Order>> GetCompletedOrdersAsync();

        // Advanced queries
        Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Order>> GetOrdersWithPaymentAsync();
        Task<Order?> GetOrderWithPaymentByIdAsync(int orderId);
        Task<IEnumerable<Order>> GetActiveOrdersAsync(); // Orders đang trong quá trình thuê
        Task<IEnumerable<Order>> GetUpcomingOrdersAsync(); // Orders sắp bắt đầu
        Task<IEnumerable<Order>> FindAsync(Expression<Func<Order, bool>> predicate);

        // User specific queries
        Task<IEnumerable<Order>> GetUserOrderHistoryAsync(int userId);
        Task<IEnumerable<Order>> GetUserActiveOrdersAsync(int userId);
        Task<Order?> GetUserLatestOrderAsync(int userId);

        // Vehicle specific queries
        Task<IEnumerable<Order>> GetVehicleBookingHistoryAsync(int vehicleId);
        Task<bool> IsVehicleAvailableAsync(int vehicleId, DateTime fromDate, DateTime toDate);
        Task<IEnumerable<Order>> GetVehicleBookingsInRangeAsync(int vehicleId, DateTime fromDate, DateTime toDate);

        // Statistics
        Task<int> GetTotalOrdersCountAsync();
        Task<int> GetOrderCountByStatusAsync(string status);
        Task<decimal> GetTotalRevenueAsync();
        Task<decimal> GetRevenueByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, int>> GetOrderCountByStatusGroupAsync();
        Task<int> GetUserOrderCountAsync(int userId);

        // Additional methods from lam branch
        Task<IEnumerable<Order>> GetOrdersByVehicleAsync(int vehicleId);
        Task<Order?> GetOrderWithPaymentAsync(int orderId);
        Task<Order?> GetOrderWithContractAsync(int orderId);
        Task<IEnumerable<Order>> GetOverlappingOrdersAsync(int vehicleId, DateTime fromDate, DateTime toDate);

        // Stage 1 Enhancement - Order expiration
        Task<IEnumerable<Order>> GetExpiredOrdersAsync();
    }
}
