using BookingSerivce.DTOs;
using BookingSerivce.Models;
using BookingSerivce.Repositories;
using BookingService.Models;

namespace BookingSerivce.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IPaymentRepository _paymentRepo;

        public OrderService(IOrderRepository orderRepo, IPaymentRepository paymentRepo)
        {
            _orderRepo = orderRepo;
            _paymentRepo = paymentRepo;
        }

        public async Task<Order> CreateOrderAsync(OrderRequest request)
        {
            // Check xe có available không
            var isAvailable = await _orderRepo.IsVehicleAvailableAsync(
                request.VehicleId,
                request.FromDate,
                request.ToDate
            );

            if (!isAvailable)
                throw new Exception("Vehicle is not available for the selected dates");

            var order = new Order
            {
                UserId = request.UserId,
                VehicleId = request.VehicleId,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                TotalDays = request.TotalTime,
                ModelPrice = request.ModelPrice,
                TotalCost = request.TotalCost,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            return await _orderRepo.AddAsync(order);
        }

        public async Task<IEnumerable<Order>> GetUserBookingsAsync(int userId)
        {
            return await _orderRepo.GetUserOrderHistoryAsync(userId);
        }

        public async Task<bool> CheckVehicleAvailabilityAsync(int vehicleId, DateTime from, DateTime to)
        {
            return await _orderRepo.IsVehicleAvailableAsync(vehicleId, from, to);
        }
    }
}
