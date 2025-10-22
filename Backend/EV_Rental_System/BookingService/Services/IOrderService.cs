﻿using BookingSerivce.DTOs;
using BookingService.DTOs;
using BookingService.Models;

namespace BookingService.Services
{
    public interface IOrderService
    {
        // (MỚI) Xem trước
        Task<OrderPreviewResponse> GetOrderPreviewAsync(OrderRequest request);

        // (Cập nhật) Tạo Order (Treo)
        Task<OrderResponse> CreateOrderAsync(OrderRequest request);

        // (Cập nhật) Xác nhận thanh toán
        Task<bool> ConfirmPaymentAsync(int orderId, string transactionId, string? gatewayResponse = null);

        // (Cập nhật) Background job
        Task<int> CheckExpiredOrdersAsync();

        // (Giữ nguyên)
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId);

        //Task<bool> CancelOrderAsync(int orderId, int userId);
        Task<bool> StartRentalAsync(int orderId);
        Task<bool> CompleteRentalAsync(int orderId);

        //Task<bool> CheckVehicleAvailabilityAsync(int vehicleId, DateTime fromDate, DateTime toDate);

        // Availability checking for microservice communication
        Task<List<ConflictingOrderDto>> GetConflictingOrdersAsync(int vehicleId, DateTime fromDate, DateTime toDate, int? excludeOrderId = null);
    }
}
