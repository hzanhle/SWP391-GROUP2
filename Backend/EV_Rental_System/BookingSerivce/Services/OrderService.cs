using BookingSerivce.DTOs;
using BookingSerivce.Models;
using BookingSerivce.Repositories;
using BookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingSerivce.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IPaymentRepository _paymentRepo;
        private readonly ISoftLockRepository _softLockRepo;
        private readonly ITrustScoreService _trustScoreService;
        private readonly MyDbContext _context;

        public OrderService(
            IOrderRepository orderRepo,
            IPaymentRepository paymentRepo,
            ISoftLockRepository softLockRepo,
            ITrustScoreService trustScoreService,
            MyDbContext context)
        {
            _orderRepo = orderRepo;
            _paymentRepo = paymentRepo;
            _softLockRepo = softLockRepo;
            _trustScoreService = trustScoreService;
            _context = context;
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

        // ========== Stage 1 Enhancement Methods ==========

        public async Task<OrderPreviewResponse> PreviewOrderAsync(OrderPreviewRequest request)
        {
            // Validate date range
            if (request.FromDate >= request.ToDate)
                throw new Exception("FromDate must be before ToDate");

            if (request.FromDate < DateTime.UtcNow.Date)
                throw new Exception("FromDate cannot be in the past");

            // Check vehicle availability (excluding soft locks initially)
            var isAvailable = await _orderRepo.IsVehicleAvailableAsync(
                request.VehicleId,
                request.FromDate,
                request.ToDate
            );

            if (!isAvailable)
                throw new Exception("Vehicle is not available for the selected dates");

            // Check for active soft locks on this vehicle for these dates
            var hasActiveLock = await _softLockRepo.HasActiveLockAsync(
                request.VehicleId,
                request.FromDate,
                request.ToDate
            );

            if (hasActiveLock)
                throw new Exception("Vehicle is currently being previewed by another user. Please try again in a moment.");

            // Create soft lock
            var softLock = new SoftLock
            {
                LockToken = Guid.NewGuid(),
                VehicleId = request.VehicleId,
                UserId = request.UserId,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5), // 5-minute expiration
                Status = "Active"
            };

            await _softLockRepo.AddAsync(softLock);

            // Get trust score
            var trustScore = await _trustScoreService.GetUserTrustScoreAsync(request.UserId);
            var depositPercentage = _trustScoreService.CalculateDepositPercentage(trustScore);

            // Calculate costs
            var totalDays = (request.ToDate - request.FromDate).TotalDays;
            var totalHours = totalDays * 24;
            var totalCost = request.HourlyRate * (decimal)totalHours;
            var depositAmount = totalCost * depositPercentage;

            return new OrderPreviewResponse
            {
                PreviewToken = softLock.LockToken,
                TotalCost = totalCost,
                DepositAmount = depositAmount,
                DepositPercentage = depositPercentage,
                TrustScore = trustScore,
                ExpiresAt = softLock.ExpiresAt,
                TotalDays = (int)Math.Ceiling(totalDays),
                ModelPrice = request.HourlyRate
            };
        }

        public async Task<OrderResponse> ConfirmOrderAsync(ConfirmOrderRequest request)
        {
            // Validate soft lock
            var softLock = await _softLockRepo.GetByTokenAsync(request.PreviewToken);

            if (softLock == null)
                throw new Exception("Invalid preview token. Please create a new preview.");

            if (softLock.Status != "Active")
                throw new Exception($"Preview token has been {softLock.Status.ToLower()}. Please create a new preview.");

            if (!softLock.IsValid())
                throw new Exception("Preview has expired. Please create a new preview.");

            // Verify lock matches request
            if (softLock.VehicleId != request.VehicleId ||
                softLock.UserId != request.UserId ||
                softLock.FromDate != request.FromDate ||
                softLock.ToDate != request.ToDate)
            {
                throw new Exception("Request data doesn't match preview. Please create a new preview.");
            }

            // Start transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Consume soft lock first
                softLock.Consume();
                await _softLockRepo.UpdateAsync(softLock);

                // Get trust score and calculate deposit
                var trustScore = await _trustScoreService.GetUserTrustScoreAsync(request.UserId);
                var depositPercentage = _trustScoreService.CalculateDepositPercentage(trustScore);

                // Recalculate costs for security
                var totalDays = (request.ToDate - request.FromDate).TotalDays;
                var totalHours = totalDays * 24;
                var totalCost = request.HourlyRate * (decimal)totalHours;
                var depositAmount = totalCost * depositPercentage;

                // Validate recalculated cost matches frontend calculation (allow small rounding difference)
                if (Math.Abs(totalCost - request.TotalCost) > 0.01m)
                {
                    throw new Exception("Cost calculation mismatch. Please create a new preview.");
                }

                // Create order
                var order = new Order
                {
                    UserId = request.UserId,
                    VehicleId = request.VehicleId,
                    FromDate = request.FromDate,
                    ToDate = request.ToDate,
                    TotalDays = (int)Math.Ceiling(totalDays),
                    ModelPrice = request.HourlyRate,
                    TotalCost = totalCost,
                    DepositAmount = depositAmount,
                    DepositPercentage = depositPercentage,
                    TrustScoreAtBooking = trustScore,
                    PreviewToken = request.PreviewToken,
                    Status = "Pending",
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5), // 5 minutes to initiate payment
                    CreatedAt = DateTime.UtcNow
                };

                var createdOrder = await _orderRepo.AddAsync(order);

                // Commit transaction
                await transaction.CommitAsync();

                return new OrderResponse
                {
                    OrderId = createdOrder.OrderId,
                    TotalCost = createdOrder.TotalCost,
                    DepositAmount = createdOrder.DepositAmount,
                    ExpiresAt = createdOrder.ExpiresAt.Value,
                    Status = createdOrder.Status,
                    TrustScore = trustScore,
                    DepositPercentage = depositPercentage
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
