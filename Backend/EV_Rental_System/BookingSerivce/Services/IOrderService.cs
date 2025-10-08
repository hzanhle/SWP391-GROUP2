using BookingSerivce.DTOs;
using BookingService.Models;

namespace BookingSerivce.Services
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(OrderRequest request);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<IEnumerable<Order>> GetUserOrdersAsync(int userId);
        Task<Order> UpdateOrderStatusAsync(int orderId, string status);
        Task<bool> CheckVehicleAvailabilityAsync(int vehicleId, DateTime fromDate, DateTime toDate);
        Task<IEnumerable<Order>> GetOverlappingOrdersAsync(int vehicleId, DateTime fromDate, DateTime toDate);
    }
}
