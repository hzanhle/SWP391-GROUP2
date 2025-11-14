using BookingService.Models;

namespace BookingService.Repositories
{
    public interface IOrderRepository
    {
        // === CRUD ===
        Task<Order> CreateAsync(Order order);
        Task<Order?> GetByIdAsync(int orderId);
        Task<Order?> GetByIdWithDetailsAsync(int orderId);
        Task<IEnumerable<Order>> GetAllAsync();
        Task<bool> UpdateAsync(Order order);
        Task<bool> DeleteAsync(int orderId);
        Task<Dictionary<int, int>> GetOrderCountByHourAsync();
        Task<List<int>> GetTopPeakHoursAsync(int topCount = 3);

        // === QUERIES ===
        Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Order>> GetByVehicleIdAsync(int vehicleId);
        Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status);
        Task<IEnumerable<Order>> GetByUserIdAndStatusAsync(int userId, OrderStatus status);

        // === AVAILABILITY ===
        Task<bool> IsVehicleAvailableAsync(int vehicleId, DateTime fromDate, DateTime toDate, int? excludeOrderId = null);
        Task<IEnumerable<Order>> GetOverlappingOrdersAsync(int vehicleId, DateTime fromDate, DateTime toDate, OrderStatus[] statuses);

        // === BUSINESS QUERIES ===
        Task<IEnumerable<Order>> GetPendingOrdersAsync();
        Task<IEnumerable<Order>> GetExpiredPendingOrdersAsync(); // ExpiresAt < now
        Task<int> GetUserCompletedOrdersCountAsync(int userId);
        Task<bool> HasCompletedOrderAsync(int userId);
    }
}
