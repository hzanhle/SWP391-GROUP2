using BookingSerivce.DTOs;
using BookingService.DTOs;
using BookingService.Models;
using BookingService.Repositories;

namespace BookingService.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IOnlineContractRepository _contractRepo;
        private readonly IPaymentRepository _paymentRepo;
        private readonly ITrustScoreRepository _trustScoreRepo;
        private readonly INotificationRepository _notificationRepo;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepo,
            IOnlineContractRepository contractRepo,
            IPaymentRepository paymentRepo,
            ITrustScoreRepository trustScoreRepo,
            INotificationRepository notificationRepo,
            ILogger<OrderService> logger)
        {
            _orderRepo = orderRepo;
            _contractRepo = contractRepo;
            _paymentRepo = paymentRepo;
            _trustScoreRepo = trustScoreRepo;
            _notificationRepo = notificationRepo;
            _logger = logger;
        }

        public async Task<OrderResponse> CreateOrderAsync(OrderRequest request)
        {
            try
            {
                // 1. Validate dates
                if (request.FromDate >= request.ToDate)
                    throw new ArgumentException("FromDate must be before ToDate");

                if (request.FromDate < DateTime.UtcNow)
                    throw new ArgumentException("Cannot book in the past");

                // 2. Verify calculations from FE
                var hours = (decimal)(request.ToDate - request.FromDate).TotalHours;
                var expectedTotalCost = hours * request.RentFeeForHour;
                var expectedDeposit = request.ModelPrice * 0.3m;

                if (Math.Abs(request.TotalRentalCost - expectedTotalCost) > 0.01m)
                    throw new ArgumentException("Total rental cost calculation mismatch");

                if (Math.Abs(request.DepositAmount - expectedDeposit) > 0.01m)
                    throw new ArgumentException("Deposit amount calculation mismatch");

                // 3. Check vehicle availability
                var isAvailable = await CheckVehicleAvailabilityAsync(
                    request.VehicleId,
                    request.FromDate,
                    request.ToDate);

                if (!isAvailable)
                    throw new InvalidOperationException("Vehicle is already booked for this period");

                // 4. Check if user needs to pay service fee (first-time user)
                var hasCompletedOrder = await _orderRepo.HasCompletedOrderAsync(request.UserId);

                // 5. Get user trust score
                var trustScore = await _trustScoreRepo.GetByUserIdAsync(request.UserId);
                var userTrustScore = trustScore?.Score ?? 0;

                // 6. Create Order with CORRECT parameters
                var order = new Order(
                    request.UserId,
                    request.VehicleId,
                    request.FromDate,
                    request.ToDate,
                    request.RentFeeForHour,      // ✅ HourlyRate
                    request.TotalRentalCost,      // ✅ TotalCost
                    request.DepositAmount,        // ✅ DepositAmount
                    userTrustScore                // ✅ Trust score
                );

                var _order = await _orderRepo.CreateAsync(order);

                _logger.LogInformation($"Order {_order.OrderId} created for User {request.UserId}");

                // 7. Create OnlineContract (Draft)
                var contractNumber = GenerateContractNumber(_order.OrderId);
                var contractFilePath = $"/contracts/{contractNumber}.pdf";

                var contract = new OnlineContract(
                    _order.OrderId,
                    contractNumber,
                    contractFilePath,
                    request.FromDate
                );

                await _contractRepo.CreateAsync(contract);
                _logger.LogInformation($"Contract {contractNumber} created for Order {_order.OrderId}");

                // 8. Create Payment record
                var totalPaymentAmount = _order.TotalCost + _order.DepositAmount;
                var payment = new Payment(_order.OrderId, totalPaymentAmount, "Pending");
                await _paymentRepo.CreateAsync(payment);

                // 9. Create Notification
                await _notificationRepo.CreateAsync(new Notification(
                    "Order Created",
                    $"Your booking #{_order.OrderId} has been created. Please complete payment of {totalPaymentAmount:N0} VND before {contract.ExpiresAt:yyyy-MM-dd HH:mm}",
                    "OrderCreated",
                    _order.OrderId,
                    null,
                    request.UserId,
                    DateTime.UtcNow
                ));

                // 10. Return response
                return new OrderResponse
                {
                    OrderId = _order.OrderId,
                    Status = _order.Status.ToString(),
                    ContractNumber = contractNumber,
                    ContractExpiresAt = contract.ExpiresAt,
                    TotalAmount = totalPaymentAmount,
                    Message = "Order created successfully. Please complete payment to confirm."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                throw;
            }
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _orderRepo.GetByIdAsync(orderId);
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId)
        {
            return await _orderRepo.GetByUserIdAsync(userId);
        }

        public async Task<bool> CancelOrderAsync(int orderId, int userId)
        {
            try
            {
                var order = await _orderRepo.GetByIdAsync(orderId);
                if (order == null)
                    throw new ArgumentException("Order not found");

                if (order.UserId != userId)
                    throw new UnauthorizedAccessException("You don't have permission to cancel this order");

                if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
                    throw new InvalidOperationException($"Cannot cancel order with status: {order.Status}");

                // Cancel order
                order.Cancel();
                await _orderRepo.UpdateAsync(order);
                _logger.LogInformation($"Order {orderId} cancelled by User {userId}");


                // Handle refund if payment exists
                var payment = await _paymentRepo.GetByOrderIdAsync(orderId);
                if (payment != null && payment.Status == PaymentStatus.Completed)
                {
                    payment.Status = PaymentStatus.Refunded;
                    await _paymentRepo.UpdateAsync(payment);
                    _logger.LogInformation($"Refund initiated for Payment {payment.PaymentId}");
                }

                // Send notification
                await _notificationRepo.CreateAsync(new Notification(
                    "Order Cancelled",
                    $"Your booking #{orderId} has been cancelled.",
                    "OrderCancelled",
                    orderId,
                    null,
                    userId,
                    DateTime.UtcNow
                ));

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error cancelling order {orderId}");
                throw;
            }
        }

        public async Task<bool> ConfirmPaymentAsync(int orderId, string transactionId, string? gatewayResponse = null)
        {
            try
            {
                var order = await _orderRepo.GetByIdAsync(orderId);
                if (order == null)
                    throw new ArgumentException("Order not found");

                if (order.Status != OrderStatus.Pending)
                    throw new InvalidOperationException($"Cannot confirm payment for order with status: {order.Status}");

                // Update order status
                order.Confirm();
                await _orderRepo.UpdateAsync(order);
                _logger.LogInformation($"Order {orderId} confirmed via payment {transactionId}");

                // Mark payment as completed
                var payment = await _paymentRepo.GetByOrderIdAsync(orderId);
                if (payment != null)
                {
                    payment.MarkAsCompleted(transactionId, gatewayResponse);
                    await _paymentRepo.UpdateAsync(payment);
                }

                // Update trust score for first-time users
                var trustScore = await _trustScoreRepo.GetByUserIdAsync(order.UserId);
                if (trustScore == null)
                {
                    // ✅ FIX: Create initial trust score for new user
                    trustScore = new TrustScore
                    {
                        UserId = order.UserId,
                        Score = 100, // Initial bonus for first payment
                        OrderId = orderId,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _trustScoreRepo.CreateAsync(trustScore);
                    _logger.LogInformation($"Created initial trust score for User {order.UserId}");
                }
                else
                {
                    var hasCompletedOrder = await _orderRepo.HasCompletedOrderAsync(order.UserId);
                    if (!hasCompletedOrder)
                    {
                        // ✅ First completed order bonus
                        trustScore.Score += 50;
                        trustScore.OrderId = orderId;
                        trustScore.CreatedAt = DateTime.UtcNow;
                        await _trustScoreRepo.UpdateScoreAsync(trustScore);
                    }
                }

                // Send notification
                await _notificationRepo.CreateAsync(new Notification(
                    "Payment Successful",
                    $"Your payment for booking #{orderId} has been confirmed. Contract signed!",
                    "PaymentSuccess",
                    orderId,
                    null,
                    order.UserId,
                    DateTime.UtcNow
                ));

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error confirming payment for order {orderId}");
                throw;
            }
        }

        public async Task<bool> StartRentalAsync(int orderId)
        {
            try
            {
                var order = await _orderRepo.GetByIdAsync(orderId);
                if (order == null)
                    throw new ArgumentException("Order not found");

                if (order.Status != OrderStatus.Confirmed)
                    throw new InvalidOperationException($"Cannot start rental for order with status: {order.Status}");

                if (DateTime.UtcNow < order.FromDate)
                    throw new InvalidOperationException("Rental period has not started yet");

                order.StartRental();
                await _orderRepo.UpdateAsync(order);
                _logger.LogInformation($"Rental started for Order {orderId}");

                await _notificationRepo.CreateAsync(new Notification(
                    "Rental Started",
                    $"Your rental for booking #{orderId} has started. Enjoy your ride!",
                    "RentalStarted",
                    orderId,
                    null,
                    order.UserId,
                    DateTime.UtcNow
                ));

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error starting rental for order {orderId}");
                throw;
            }
        }

        public async Task<bool> CompleteRentalAsync(int orderId)
        {
            try
            {
                var order = await _orderRepo.GetByIdAsync(orderId);
                if (order == null)
                    throw new ArgumentException("Order not found");

                if (order.Status != OrderStatus.InProgress)
                    throw new InvalidOperationException($"Cannot complete order with status: {order.Status}");

                order.Complete();
                await _orderRepo.UpdateAsync(order);
                _logger.LogInformation($"Rental completed for Order {orderId}");

                // Increase trust score
                var trustScore = await _trustScoreRepo.GetByUserIdAsync(order.UserId);
                if (trustScore != null)
                {
                    trustScore.Score += 5; // Bonus for completing rental
                    trustScore.OrderId = orderId;
                    trustScore.CreatedAt = DateTime.UtcNow;
                    await _trustScoreRepo.UpdateScoreAsync(trustScore);
                }

                await _notificationRepo.CreateAsync(new Notification(
                    "Rental Completed",
                    $"Your rental for booking #{orderId} has been completed. Please leave a review!",
                    "RentalCompleted",
                    orderId,
                    null,
                    order.UserId,
                    DateTime.UtcNow
                ));

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error completing rental for order {orderId}");
                throw;
            }
        }

        public async Task<int> CheckExpiredContractsAsync()
        {
            try
            {
                var expiredContracts = await _contractRepo.GetExpiredContractsAsync();
                int count = 0;

                foreach (var contract in expiredContracts)
                {
                    var order = await _orderRepo.GetByIdAsync(contract.OrderId);
                    if (order != null && order.Status == OrderStatus.Pending)
                    {
                        order.Cancel();
                        await _orderRepo.UpdateAsync(order);

                        await _notificationRepo.CreateAsync(new Notification(
                            "Order Expired",
                            $"Your booking #{order.OrderId} has expired due to non-payment.",
                            "OrderExpired",
                            order.OrderId,
                            null,
                            order.UserId,
                            DateTime.UtcNow
                        ));

                        count++;
                        _logger.LogInformation($"Order {order.OrderId} auto-cancelled due to expired contract");
                    }
                }

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking expired contracts");
                throw;
            }
        }

        public async Task<decimal> CalculateTotalCostAsync(decimal rentFeePerHour, DateTime fromDate, DateTime toDate)
        {
            if (fromDate >= toDate)
                throw new ArgumentException("FromDate must be before ToDate");

            var hours = (decimal)(toDate - fromDate).TotalHours;
            return hours * rentFeePerHour;
        }

        public async Task<bool> CheckVehicleAvailabilityAsync(int vehicleId, DateTime fromDate, DateTime toDate)
        {
            var overlappingOrders = await _orderRepo.GetOverlappingOrdersAsync(
                vehicleId,
                fromDate,
                toDate,
                new[] { OrderStatus.Confirmed, OrderStatus.InProgress }
            );

            return !overlappingOrders.Any();
        }

        private string GenerateContractNumber(int orderId)
        {
            var year = DateTime.UtcNow.Year;
            return $"CT-{year}-{orderId:D6}";
        }
    }
}