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

        // Stage 2 Enhancement - Payment confirmation and contract generation
        /// <summary>
        /// Confirms payment and automatically generates contract.
        /// Called by VNPay webhook after successful payment.
        /// </summary>
        Task<OrderPaymentConfirmationResponse> ConfirmPaymentAsync(int orderId);

        // Stage 3 Enhancement - Pickup and Return Management
        /// <summary>
        /// Confirms vehicle pickup by staff. Updates status to InProgress.
        /// </summary>
        Task<Order> ConfirmPickupAsync(ConfirmPickupRequest request);

        /// <summary>
        /// Confirms vehicle return by staff. Updates status to Returned.
        /// </summary>
        Task<Order> ConfirmReturnAsync(ConfirmReturnRequest request);

        /// <summary>
        /// Gets order status with role-based display information.
        /// </summary>
        Task<OrderStatusResponse> GetOrderStatusAsync(int orderId, string userRole, int requestingUserId);
    }
}
