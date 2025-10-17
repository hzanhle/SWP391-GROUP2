using BookingSerivce.DTOs;
using BookingService.DTOs;
using BookingService.Models;

namespace BookingService.Services
{
    public interface IOrderService
    {
        Task<OrderResponse> CreateOrderAsync(OrderRequest request);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId);
        Task<bool> CancelOrderAsync(int orderId, int userId);
        Task<bool> ConfirmPaymentAsync(int orderId, string transactionId, string? gatewayResponse = null);
        Task<bool> StartRentalAsync(int orderId);
        Task<bool> CompleteRentalAsync(int orderId);
        Task<int> CheckExpiredContractsAsync();
        Task<decimal> CalculateTotalCostAsync(decimal rentFeePerHour, DateTime fromDate, DateTime toDate);
        Task<bool> CheckVehicleAvailabilityAsync(int vehicleId, DateTime fromDate, DateTime toDate);
    }
}
