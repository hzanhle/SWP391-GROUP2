using BookingSerivce.Models;
using BookingService.Models;
using System.Linq.Expressions;

namespace BookingSerivce.Repositories
{
    public interface IOrderRepository
    {
        // ===== BASIC CRUD =====
        Task<Order?> GetByIdAsync(int orderId);
        Task<IEnumerable<Order>> GetAllAsync();
        Task<Order> AddAsync(Order order);
        Task<Order> UpdateAsync(Order order);
        Task DeleteAsync(int orderId);
        Task<bool> ExistsAsync(int orderId);

        // ===== QUERY METHODS =====
        Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Order>> GetByVehicleIdAsync(int vehicleId);
        Task<IEnumerable<Order>> GetByStatusAsync(string status);
        Task<IEnumerable<Order>> GetPendingOrdersAsync();
        Task<IEnumerable<Order>> GetConfirmedOrdersAsync();
        Task<IEnumerable<Order>> GetCompletedOrdersAsync();

        // ===== ADVANCED QUERIES =====
        Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Order>> GetOrdersWithPaymentAsync();
        Task<Order?> GetOrderWithPaymentAsync(int orderId);
        Task<Order?> GetOrderWithContractAsync(int orderId);
        Task<IEnumerable<Order>> GetActiveOrdersAsync();
        Task<IEnumerable<Order>> GetUpcomingOrdersAsync();
        Task<IEnumerable<Order>> FindAsync(Expression<Func<Order, bool>> predicate);

        // ===== USER SPECIFIC QUERIES =====
        Task<IEnumerable<Order>> GetUserOrderHistoryAsync(int userId);
        Task<IEnumerable<Order>> GetUserActiveOrdersAsync(int userId);
        Task<Order?> GetUserLatestOrderAsync(int userId);

        // ===== VEHICLE SPECIFIC QUERIES =====
        Task<IEnumerable<Order>> GetOrdersByVehicleAsync(int vehicleId);
        Task<IEnumerable<Order>> GetVehicleBookingHistoryAsync(int vehicleId);
        Task<bool> IsVehicleAvailableAsync(int vehicleId, DateTime fromDate, DateTime toDate);
        Task<IEnumerable<Order>> GetVehicleBookingsInRangeAsync(int vehicleId, DateTime fromDate, DateTime toDate);
        Task<IEnumerable<Order>> GetOverlappingOrdersAsync(int vehicleId, DateTime fromDate, DateTime toDate);

        // ===== STATISTICS =====
        Task<int> GetTotalOrdersCountAsync();
        Task<int> GetOrderCountByStatusAsync(string status);
        Task<decimal> GetTotalRevenueAsync();
        Task<decimal> GetRevenueByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, int>> GetOrderCountByStatusGroupAsync();
        Task<int> GetUserOrderCountAsync(int userId);

        // Stage 1 Enhancement - Order expiration
        Task<IEnumerable<Order>> GetExpiredOrdersAsync();
    }
}