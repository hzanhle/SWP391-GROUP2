using BookingSerivce.DTOs;
using BookingSerivce.Hubs;
using BookingSerivce.Models;
using BookingSerivce.Repositories;
using BookingService.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BookingSerivce.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IPaymentRepository _paymentRepo;
        private readonly ISoftLockRepository _softLockRepo;
        private readonly ITrustScoreService _trustScoreService;
        private readonly IContractService _contractService;
        private readonly INotificationService _notificationService;
        private readonly IOrderStatusMapper _statusMapper;
        private readonly IHubContext<OrderHub> _hubContext;
        private readonly MyDbContext _context;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepo,
            IPaymentRepository paymentRepo,
            ISoftLockRepository softLockRepo,
            ITrustScoreService trustScoreService,
            IContractService contractService,
            INotificationService notificationService,
            IOrderStatusMapper statusMapper,
            IHubContext<OrderHub> hubContext,
            MyDbContext context,
            ILogger<OrderService> logger)
        {
            _orderRepo = orderRepo;
            _paymentRepo = paymentRepo;
            _softLockRepo = softLockRepo;
            _trustScoreService = trustScoreService;
            _contractService = contractService;
            _notificationService = notificationService;
            _statusMapper = statusMapper;
            _hubContext = hubContext;
            _context = context;
            _logger = logger;
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
            var validStatuses = new[] {
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
                    ExpiresAt = createdOrder.ExpiresAt ?? DateTime.UtcNow.AddMinutes(5),
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

        // ===== Stage 2 Enhancement Methods =====

        /// <summary>
        /// Confirms payment and automatically generates contract.
        /// This is called after VNPay webhook confirms successful payment.
        /// </summary>
        public async Task<OrderPaymentConfirmationResponse> ConfirmPaymentAsync(int orderId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Get and validate order
                var order = await _orderRepo.GetByIdAsync(orderId);
                if (order == null)
                    throw new InvalidOperationException($"Order {orderId} not found");

                // 2. Get and validate payment
                var payment = await _paymentRepo.GetByOrderIdAsync(orderId);
                if (payment == null)
                    throw new InvalidOperationException($"Payment not found for order {orderId}");

                if (payment.Status != "Completed")
                    throw new InvalidOperationException($"Payment status is {payment.Status}, expected Completed");

                // 3. Update order status to Confirmed
                order.Status = "Confirmed";
                order.ConfirmedAt = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;
                await _orderRepo.UpdateAsync(order);

                _logger.LogInformation("Order {OrderId} confirmed after payment", orderId);

                // 4. Automatically generate contract with PDF
                var contract = await _contractService.GenerateContractWithPdfAsync(orderId);

                _logger.LogInformation("Contract {ContractId} generated for Order {OrderId}",
                    contract.ContractId, orderId);

                // 5. Update order with contract info
                order.ContractId = contract.ContractId;
                order.ContractGeneratedAt = DateTime.UtcNow;
                order.Status = "ContractGenerated";
                await _orderRepo.UpdateAsync(order);

                // 6. Update trust score (if first order)
                var userOrders = await _orderRepo.GetUserOrderHistoryAsync(order.UserId);
                if (userOrders.Count() == 1) // This is first completed order
                {
                    await _trustScoreService.GetUserTrustScoreAsync(order.UserId); // Trigger recalculation
                }

                // 7. Send notification
                await _notificationService.AddNotification(new NotificationRequest
                {
                    UserId = order.UserId,
                    Title = "Payment Successful!",
                    Description = $"Your payment has been confirmed. Contract {contract.ContractNumber} has been generated.",
                    DataType = "Order",
                    DataId = orderId
                });

                await transaction.CommitAsync();

                // 8. Return response
                return new OrderPaymentConfirmationResponse
                {
                    Success = true,
                    OrderId = orderId,
                    PaymentId = payment.PaymentId,
                    ContractId = contract.ContractId,
                    ContractNumber = contract.ContractNumber,
                    ContractPdfUrl = contract.PdfFilePath ?? string.Empty,
                    Message = "Payment confirmed and contract generated successfully",
                    ConfirmedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to confirm payment for Order {OrderId}", orderId);
                throw;
            }
        }

        // ===== Stage 3 Enhancement Methods =====

        /// <summary>
        /// Confirms vehicle pickup by staff. Changes status from ContractGenerated to InProgress.
        /// </summary>
        public async Task<Order> ConfirmPickupAsync(ConfirmPickupRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _orderRepo.GetByIdAsync(request.OrderId);
                if (order == null)
                    throw new InvalidOperationException($"Order {request.OrderId} not found");

                // Validate status transition
                if (!_statusMapper.IsValidStatusTransition(order.Status, "InProgress"))
                    throw new InvalidOperationException($"Cannot confirm pickup for order with status {order.Status}");

                // Update order with pickup information
                order.Status = "InProgress";
                order.ActualPickupTime = request.ActualPickupTime;
                order.PickupOdometerReading = request.OdometerReading;
                order.PickupBatteryLevel = request.BatteryLevel;
                order.PickupNotes = request.Notes;
                order.HandedOverByStaffId = request.StaffId;
                order.UpdatedAt = DateTime.UtcNow;

                await _orderRepo.UpdateAsync(order);
                await transaction.CommitAsync();

                _logger.LogInformation("Pickup confirmed for Order {OrderId} by Staff {StaffId}",
                    request.OrderId, request.StaffId);

                // Send SignalR notification to customer
                await _hubContext.Clients.Group($"user_{order.UserId}")
                    .SendAsync("PickupConfirmed", new
                    {
                        OrderId = order.OrderId,
                        Message = $"Your vehicle is ready! Vehicle has been prepared for you.",
                        PickupTime = request.ActualPickupTime,
                        Timestamp = DateTime.UtcNow
                    });

                return order;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to confirm pickup for Order {OrderId}", request.OrderId);
                throw;
            }
        }

        /// <summary>
        /// Confirms vehicle return by staff. Changes status from InProgress to Returned.
        /// </summary>
        public async Task<Order> ConfirmReturnAsync(ConfirmReturnRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _orderRepo.GetByIdAsync(request.OrderId);
                if (order == null)
                    throw new InvalidOperationException($"Order {request.OrderId} not found");

                // Validate status transition
                if (!_statusMapper.IsValidStatusTransition(order.Status, "Returned"))
                    throw new InvalidOperationException($"Cannot confirm return for order with status {order.Status}");

                // Update order with return information
                order.Status = "Returned";
                order.ActualReturnTime = request.ActualReturnTime;
                order.ReturnOdometerReading = request.OdometerReading;
                order.ReturnBatteryLevel = request.BatteryLevel;
                order.ReturnNotes = request.Notes;
                order.ReceivedByStaffId = request.StaffId;
                order.UpdatedAt = DateTime.UtcNow;

                // Check if return is late
                if (request.ActualReturnTime > order.ToDate)
                {
                    order.IsLateReturn = true;
                    var lateDuration = request.ActualReturnTime - order.ToDate;
                    order.LateReturnHours = (int)Math.Ceiling(lateDuration.TotalHours);
                    // Calculate late fee (assuming hourly rate from order)
                    order.LateFee = order.LateReturnHours.Value * order.ModelPrice;
                }

                await _orderRepo.UpdateAsync(order);
                await transaction.CommitAsync();

                _logger.LogInformation("Return confirmed for Order {OrderId} by Staff {StaffId}. Late: {IsLate}",
                    request.OrderId, request.StaffId, order.IsLateReturn);

                // Send SignalR notification to customer
                await _hubContext.Clients.Group($"user_{order.UserId}")
                    .SendAsync("ReturnConfirmed", new
                    {
                        OrderId = order.OrderId,
                        Message = "Vehicle return confirmed. Inspection will begin shortly.",
                        ReturnTime = request.ActualReturnTime,
                        IsLateReturn = order.IsLateReturn,
                        LateFee = order.LateFee,
                        Timestamp = DateTime.UtcNow
                    });

                return order;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to confirm return for Order {OrderId}", request.OrderId);
                throw;
            }
        }

        /// <summary>
        /// Gets order status with role-based display information.
        /// </summary>
        public async Task<OrderStatusResponse> GetOrderStatusAsync(int orderId, string userRole, int requestingUserId)
        {
            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order == null)
                throw new InvalidOperationException($"Order {orderId} not found");

            // Verify user has permission to view this order
            var isAdmin = userRole == "Admin" || userRole == "Staff";
            if (!isAdmin && order.UserId != requestingUserId)
                throw new UnauthorizedAccessException("You do not have permission to view this order");

            var response = new OrderStatusResponse
            {
                OrderId = order.OrderId,
                InternalStatus = order.Status,
                DisplayStatus = isAdmin
                    ? _statusMapper.GetAdminDisplayStatus(order.Status)
                    : _statusMapper.GetCustomerDisplayStatus(order.Status),
                CanModifyStatus = isAdmin,
                AvailableActions = isAdmin
                    ? _statusMapper.GetAvailableActions(order.Status).ToList()
                    : new List<string>(),

                // Scheduled times
                ScheduledPickupTime = order.FromDate,
                ScheduledReturnTime = order.ToDate,

                // Late return info (visible to both)
                IsLateReturn = order.IsLateReturn,
                LateReturnHours = order.LateReturnHours,
                LateFee = order.LateFee
            };

            // Add detailed tracking info for admin/staff only
            if (isAdmin)
            {
                response.ActualPickupTime = order.ActualPickupTime;
                response.ActualReturnTime = order.ActualReturnTime;
                response.PickupOdometerReading = order.PickupOdometerReading;
                response.ReturnOdometerReading = order.ReturnOdometerReading;
                response.PickupBatteryLevel = order.PickupBatteryLevel;
                response.ReturnBatteryLevel = order.ReturnBatteryLevel;
                response.HandedOverByStaffId = order.HandedOverByStaffId;
                response.ReceivedByStaffId = order.ReceivedByStaffId;
                response.HasDamage = order.HasDamage;
                response.DamageCharge = order.DamageCharge;
            }

            return response;
        }
    }
}
