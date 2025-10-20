using BookingSerivce.DTOs;
using BookingService.Models;

namespace BookingSerivce.Services
{
    public interface IOrderService
    {
        // Legacy methods (kept for backward compatibility)
        Task<Order> CreateOrderAsync(OrderRequest request);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<IEnumerable<Order>> GetUserOrdersAsync(int userId);
        Task<Order> UpdateOrderStatusAsync(int orderId, string status);
        Task<bool> CheckVehicleAvailabilityAsync(int vehicleId, DateTime fromDate, DateTime toDate);
        Task<IEnumerable<Order>> GetOverlappingOrdersAsync(int vehicleId, DateTime fromDate, DateTime toDate);

        // Stage 1 Enhancement - Preview and Confirm flow
        /// <summary>
        /// Creates a preview of an order with calculated costs and soft lock.
        /// Returns a preview token valid for 5 minutes.
        /// </summary>
        Task<OrderPreviewResponse> PreviewOrderAsync(OrderPreviewRequest request);

        /// <summary>
        /// Confirms an order using a preview token.
        /// Validates soft lock, recalculates costs, and creates order.
        /// </summary>
        Task<OrderResponse> ConfirmOrderAsync(ConfirmOrderRequest request);
    }
}
