using BookingService.Models;

namespace BookingService.Repositories
{
    public interface IOrderRepository
    {
        // === CREATE ===
        Task<Order> CreateAsync(Order order);

        // === READ ===
        Task<Order?> GetByIdAsync(int orderId);
        Task<Order?> GetByIdWithDetailsAsync(int orderId); // Include Payment & Contract
        Task<List<Order>> GetByUserIdAsync(int userId);
        Task<List<Order>> GetByVehicleIdAsync(int vehicleId);
        Task<List<Order>> GetByStatusAsync(OrderStatus status);
        Task<List<Order>> GetAllAsync();

        // === UPDATE ===
        Task<Order> UpdateAsync(Order order);

        // === DELETE ===
        Task DeleteAsync(int orderId);

        // === AVAILABILITY CHECK ===
        Task<bool> IsVehicleAvailableAsync(int vehicleId, DateTime fromDate, DateTime toDate, int? excludeOrderId = null);
        Task<List<Order>> GetConflictingOrdersAsync(int vehicleId, DateTime fromDate, DateTime toDate);

        // === BUSINESS QUERIES ===
        Task<List<Order>> GetPendingOrdersAsync(); // Orders chưa thanh toán
        Task<List<Order>> GetExpiredPendingOrdersAsync(int minutesThreshold); // Orders pending quá lâu
        Task<int> GetUserCompletedOrdersCountAsync(int userId); // Đếm số orders completed của user
        Task<bool> HasCompletedOrderAsync(int userId);
        Task<IEnumerable<Order>> GetByUserIdAndStatusAsync(int userId, OrderStatus status);
        Task<IEnumerable<Order>> GetOverlappingOrdersAsync(
            int vehicleId,
            DateTime fromDate,
            DateTime toDate,
            OrderStatus[] statuses);
    }
}
