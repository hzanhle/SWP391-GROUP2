using BookingService.Models;

namespace BookingService.Repositories
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(int orderId);
        Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
        Task<Order> CreateAsync(Order order);
        Task<bool> UpdateAsync(Order order);
        Task<bool> HasCompletedOrderAsync(int userId);
        Task<IEnumerable<Order>> GetOverlappingOrdersAsync(int vehicleId, DateTime fromDate, DateTime toDate, OrderStatus[] statuses);

        /// <summary>
        /// (MỚI) Lấy các order Pending đã hết hạn (ExpiresAt < now).
        /// </summary>
        Task<IEnumerable<Order>> GetExpiredPendingOrdersAsync();
    }
}
