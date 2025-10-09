using BookingSerivce.DTOs;
using BookingSerivce.Models;
using BookingSerivce.Repositories;
using BookingService.Models;

namespace BookingSerivce.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;

        public OrderService(IOrderRepository orderRepo)
        {
            _orderRepo = orderRepo;
        }

        public async Task<Order> CreateOrderAsync(OrderRequest request)
        {
            // Validate date range
            if (request.FromDate >= request.ToDate)
                throw new Exception("FromDate must be before ToDate");

            if (request.FromDate < DateTime.UtcNow)
                throw new Exception("FromDate cannot be in the past");

            // Check xe có available không
            var isAvailable = await _orderRepo.IsVehicleAvailableAsync(
                request.VehicleId,
                request.FromDate,
                request.ToDate
            );

            if (!isAvailable)
                throw new Exception("Vehicle is not available for the selected dates");

            // Calculate deposit amount (30% of total cost)
            var depositAmount = request.TotalCost * 0.3m;

            var order = new Order
            {
                UserId = request.UserId,
                VehicleId = request.VehicleId,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                TotalDays = request.TotalTime,
                ModelPrice = request.ModelPrice,
                TotalCost = request.TotalCost,
                DepositAmount = depositAmount,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            return await _orderRepo.AddAsync(order);
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _orderRepo.GetByIdAsync(orderId);
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId)
        {
            return await _orderRepo.GetUserOrderHistoryAsync(userId);
        }

        public async Task<Order> UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order == null)
                throw new Exception("Order not found");

            // Validate status transitions
            var validStatuses = new[]
            {
                "Pending", "AwaitingContract", "ContractSigned",
                "AwaitingDeposit", "DepositPaid", "Confirmed",
                "InProgress", "Completed", "Cancelled"
            };

            if (!validStatuses.Contains(status))
                throw new Exception($"Invalid order status: {status}");

            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;

            return await _orderRepo.UpdateAsync(order);
        }

        public async Task<bool> CheckVehicleAvailabilityAsync(int vehicleId, DateTime fromDate, DateTime toDate)
        {
            return await _orderRepo.IsVehicleAvailableAsync(vehicleId, fromDate, toDate);
        }

        public async Task<IEnumerable<Order>> GetOverlappingOrdersAsync(int vehicleId, DateTime fromDate, DateTime toDate)
        {
            return await _orderRepo.GetOverlappingOrdersAsync(vehicleId, fromDate, toDate);
        }
    }
}
