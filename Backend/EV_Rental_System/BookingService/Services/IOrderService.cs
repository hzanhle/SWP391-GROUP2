using BookingSerivce.DTOs;
using BookingService.DTOs;
using BookingService.Models;

namespace BookingService.Services
{
    public interface IOrderService
    {
        // (MỚI) Xem trước
        Task<OrderPreviewResponse> GetOrderPreviewAsync(OrderRequest request, int userId);

        // (Cập nhật) Tạo Order (Treo)
        Task<OrderResponse> CreateOrderAsync(OrderRequest request, int userId);

        // (Cập nhật) Xác nhận thanh toán
        Task<bool> ConfirmPaymentAsync(int orderId, string transactionId, string? gatewayResponse = null);

        // (Cập nhật) Background job
        Task<int> CheckExpiredOrdersAsync();

        Task<PeakHoursReportResponse> GetPeakHoursReportAsync();

        Task UpdateOrderAsync(Order order);


        // (Giữ nguyên)
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<Order?> GetOrderByIdWithDetailsAsync(int orderId);
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId);

        //Task<bool> CancelOrderAsync(int orderId, int userId);
        Task<bool> StartRentalAsync(int orderId, List<IFormFile> images, int confirmedBy, VehicleCheckInRequest request);
        Task<bool> CompleteRentalAsync(int orderId, List<IFormFile> images, int confirmedBy, VehicleReturnRequest request);

        //Task<bool> CheckVehicleAvailabilityAsync(int vehicleId, DateTime fromDate, DateTime toDate);
    }
}
