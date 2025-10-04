using BookingService.Models;

namespace BookingSerivce.Repositories
{
    public interface IOrderRepository
    {
        Task<Order> AddAsync(Order order);
        Task<Order?> GetByIdAsync(int orderId);
        Task<IEnumerable<Order>> GetUserOrderHistoryAsync(int userId);
        Task<Order> UpdateAsync(Order order);
        Task<bool> IsVehicleAvailableAsync(int vehicleId, DateTime fromDate, DateTime toDate);
        Task<IEnumerable<Order>> GetOrdersByVehicleAsync(int vehicleId);
        Task<Order?> GetOrderWithPaymentAsync(int orderId);
        Task<Order?> GetOrderWithContractAsync(int orderId);
        Task<IEnumerable<Order>> GetOverlappingOrdersAsync(int vehicleId, DateTime fromDate, DateTime toDate);
    }
}
