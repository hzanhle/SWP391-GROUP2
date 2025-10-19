using BookingService.DTOs;
using BookingService.Models;        // Only for Order model
using BookingService.Repositories;    // Only for IOrderRepository
using BookingService.Services.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace BookingService.Services
{
    public class OrderService : IOrderService
    {
        // --- Only Order Repository ---
        private readonly IOrderRepository _orderRepo;

        // --- Injected Services (as per your examples) ---
        private readonly IOnlineContractService _contractService;
        private readonly IPaymentService _paymentService;             // Service Layer
        private readonly INotificationService _notificationService;   // Service Layer
        private readonly ITrustScoreService _trustScoreService;       // Service Layer

        // --- SignalR & Logging ---
        private readonly IHubContext<OrderTimerHub> _hubContext;
        private readonly ILogger<OrderService> _logger;

        // --- Constants ---
        private const decimal FirstTimeUserFee = 50000m; // Example
        private const int OrderExpirationMinutes = 30;

        public OrderService(
            IOrderRepository orderRepo, // Own Repo
            IOnlineContractService contractService,
            IPaymentService paymentService,             // Inject Service
            INotificationService notificationService,   // Inject Service
            ITrustScoreService trustScoreService,       // Inject Service
            IHubContext<OrderTimerHub> hubContext,
            ILogger<OrderService> logger)
        {
            _orderRepo = orderRepo;
            _contractService = contractService;
            _paymentService = paymentService;
            _notificationService = notificationService;
            _trustScoreService = trustScoreService;
            _hubContext = hubContext;
            _logger = logger;
        }

        // --- 1. GET ORDER PREVIEW ---
        // (No changes needed, this logic is self-contained or uses OrderRepo correctly)
        public async Task<OrderPreviewResponse> GetOrderPreviewAsync(OrderRequest request)
        {
            _logger.LogInformation("Getting order preview for User {UserId}, Vehicle {VehicleId}", request.UserId, request.VehicleId);
            if (request.FromDate >= request.ToDate) throw new ArgumentException("FromDate must be before ToDate");
            if (request.FromDate < DateTime.UtcNow) throw new ArgumentException("Cannot book in the past");

            var isOverlapping = await CheckForOverlappingOrdersAsync(request.VehicleId, request.FromDate, request.ToDate);

            var hours = Math.Max(1, (decimal)(request.ToDate - request.FromDate).TotalHours); // Ensure minimum 1 hour if logic requires
            var totalRentalCost = hours * request.RentFeeForHour;
            var depositAmount = request.ModelPrice * 0.3m;
            var hasCompletedOrder = await _orderRepo.HasCompletedOrderAsync(request.UserId);
            var serviceFee = hasCompletedOrder ? 0m : FirstTimeUserFee;
            var totalPaymentAmount = totalRentalCost + depositAmount + serviceFee;

            return new OrderPreviewResponse
            {
                UserId = request.UserId,
                VehicleId = request.VehicleId,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                TotalRentalCost = totalRentalCost,
                DepositAmount = depositAmount,
                ServiceFee = serviceFee,
                TotalPaymentAmount = totalPaymentAmount,
                IsAvailable = !isOverlapping,
                Message = isOverlapping ? "Warning: Vehicle might be booked." : "Preview calculated."
            };
        }

        // --- 2. CREATE ORDER (Calls Services with Parameters) ---
        public async Task<OrderResponse> CreateOrderAsync(OrderRequest request)
        {
            _logger.LogInformation("Creating order for User {UserId}, Vehicle {VehicleId}", request.UserId, request.VehicleId);
            // NOTE: Assumes transaction is handled OUTSIDE this method (e.g., Unit of Work)

            // --- Validation & Overlap Check ---
            if (request.FromDate >= request.ToDate) throw new ArgumentException("FromDate must be before ToDate");
            if (request.FromDate < DateTime.UtcNow) throw new ArgumentException("Cannot book start date in the past");
            var isOverlapping = await CheckForOverlappingOrdersAsync(request.VehicleId, request.FromDate, request.ToDate);
            if (isOverlapping) throw new InvalidOperationException("Vehicle has just been booked.");

            // --- Calculate BE Financials ---
            var hours = Math.Max(1, (decimal)(request.ToDate - request.FromDate).TotalHours);
            var be_TotalRentalCost = hours * request.RentFeeForHour;
            var be_DepositAmount = request.ModelPrice * 0.3m;
            var hasCompletedOrder = await _orderRepo.HasCompletedOrderAsync(request.UserId);
            var be_ServiceFee = hasCompletedOrder ? 0m : FirstTimeUserFee;
            var be_TotalPaymentAmount = be_TotalRentalCost + be_DepositAmount + be_ServiceFee;
            _logger.LogInformation("BE calculated final total payment: {Amount}", be_TotalPaymentAmount);

            // --- Call TrustScore Service (to get score) ---
            var trustScore = await _trustScoreService.GetCurrentScoreAsync(request.UserId); // Use method from your TrustScoreService

            // --- Create Order (Own Repo) ---
            var order = new Order(
                request.UserId, request.VehicleId, request.FromDate, request.ToDate,
                request.RentFeeForHour, be_TotalRentalCost, be_DepositAmount, trustScore
            );
            order.ExpiresAt = DateTime.UtcNow.AddMinutes(OrderExpirationMinutes);
            // CreateAsync should just add to context, not save changes here
            var createdOrder = await _orderRepo.CreateAsync(order);
            // We might not have the ID yet if CreateAsync doesn't force save. Assuming it does for logging.
            _logger.LogInformation("Order {OrderId} added to context, expires at {ExpiresAt}", createdOrder.OrderId, order.ExpiresAt);

            // --- Call Payment Service (Create Pending) ---
            // Assuming CreatePaymentForOrderAsync matches your PaymentService example
            // Pass simple parameters, let PaymentService create the Payment model
            await _paymentService.CreatePaymentForOrderAsync(
                createdOrder.OrderId,
                be_TotalPaymentAmount,
                "PendingMethod" // Or get method from request if applicable
            );
            _logger.LogInformation("Pending payment creation requested via PaymentService for Order {OrderId}", createdOrder.OrderId);

            // --- Call Notification Service (Create Notification) ---
            // Pass parameters, let NotificationService create the Notification model
            try
            {
                await _notificationService.CreateNotificationAsync( // Use method from your NotificationService example
                    userId: request.UserId,
                    title: "Order Created",
                    description: $"Your booking #{createdOrder.OrderId} created. Please pay {be_TotalPaymentAmount:N0} VND before {order.ExpiresAt:HH:mm dd/MM/yyyy}.",
                    dataType: "OrderCreated",
                    dataId: createdOrder.OrderId,
                    staffId: null
                );
                _logger.LogInformation("Notification request sent via NotificationService for Order {OrderId} creation.", createdOrder.OrderId);
            }
            catch (Exception nex) { _logger.LogError(nex, "Failed to send notification via NotificationService during Order {OrderId} creation.", createdOrder.OrderId); }

            // --- Return Response ---
            // IMPORTANT: If CreateAsync doesn't save, createdOrder.OrderId might be 0 here.
            // The actual save/commit needs to happen *after* this method returns successfully.
            return new OrderResponse
            {
                OrderId = createdOrder.OrderId, // Relies on ID being set (might need adjustment based on UoW)
                Status = createdOrder.Status.ToString(),
                TotalAmount = be_TotalPaymentAmount,
                ExpiresAt = order.ExpiresAt,
                Message = "Order created successfully. Please complete payment."
            };
            // The caller (e.g., Controller with UoW) would call SaveChanges here
        }

        // --- 3. CONFIRM PAYMENT (Calls Services) ---
        public async Task<bool> ConfirmPaymentAsync(int orderId, string transactionId, string? gatewayResponse = null)
        {
            _logger.LogInformation("Confirming payment for Order {OrderId}", orderId);
            // NOTE: Assumes transaction is handled OUTSIDE this method

            // --- Get and Update Order (Own Repo) ---
            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order == null) throw new ArgumentException("Order not found");
            if (order.Status != OrderStatus.Pending)
            {
                if (order.Status == OrderStatus.Confirmed) return true; // Idempotent
                throw new InvalidOperationException($"Cannot confirm payment for order with status: {order.Status}");
            }
            order.Confirm(); // Update local entity status
            // UpdateAsync should just mark entity as modified in context
            await _orderRepo.UpdateAsync(order);
            _logger.LogInformation("Local Order {OrderId} status updated to Confirmed in context.", orderId);

            // --- Call Payment Service (Mark Completed) ---
            // Use method from your PaymentService example
            // This service will fetch Payment, call payment.MarkAsCompleted, and call repo.UpdateAsync
            var paymentSuccess = await _paymentService.MarkPaymentCompletedAsync(orderId, transactionId, gatewayResponse);
            if (!paymentSuccess)
            {
                _logger.LogError("PaymentService failed to mark payment as completed for Order {OrderId}. Transaction might need rollback.", orderId);
                // If this fails, the transaction should ideally rollback the order status change too.
                throw new InvalidOperationException($"Payment service failed for Order {orderId}");
            }
            _logger.LogInformation("Payment completion requested via PaymentService for Order {OrderId}.", orderId);

            // Retrieve the completed payment details if needed by other services (like contract)
            var completedPayment = await _paymentService.GetPaymentByOrderIdAsync(orderId); // Use method from your PaymentService
            if (completedPayment == null || completedPayment.Status != PaymentStatus.Completed)
            {
                _logger.LogCritical("CRITICAL: Payment marked completed by service, but GetPaymentByOrderIdAsync returned invalid state for Order {OrderId}.", orderId);
                // Cannot proceed reliably to contract generation
                throw new Exception($"Inconsistent payment state after confirmation for Order {orderId}");
            }


            // --- Call Contract Service ---
            try
            {
                // Pass necessary IDs and the confirmed Payment object/DTO
                await _contractService.GenerateAndSendContractAsync(
                    order.OrderId, order.UserId, order.VehicleId, completedPayment);
                _logger.LogInformation("Contract generation request sent via OnlineContractService for Order {OrderId}.", orderId);
            }
            catch (Exception contractEx) { _logger.LogCritical(contractEx, "CRITICAL: Payment {OrderId} SUCCESS, but FAILED contract generation.", orderId); }

            // --- Call TrustScore Service ---
            try
            {
                // Use method from your TrustScoreService example
                await _trustScoreService.UpdateScoreOnFirstPaymentAsync(order.UserId, orderId);
                _logger.LogInformation("Trust score update requested for first payment via TrustScoreService for User {UserId}, Order {OrderId}.", order.UserId, orderId);
            }
            catch (Exception tsEx) { _logger.LogError(tsEx, "Failed to update trust score via TrustScoreService for User {UserId}, Order {OrderId}.", order.UserId, orderId); }


            // --- Call Notification Service ---
            Notification createdNotification = null;
            try
            {
                // Use method from your NotificationService example
                createdNotification = await _notificationService.CreateNotificationAsync(
                    userId: order.UserId,
                    title: "Payment Successful",
                    description: $"Payment for booking #{orderId} confirmed. Contract sent to your email.",
                    dataType: "PaymentSuccess",
                    dataId: orderId,
                    staffId: null
                );
                _logger.LogInformation("Payment success notification request sent via NotificationService for Order {OrderId}.", orderId);

                // --- Send SignalR ---
                if (createdNotification != null) // Check if notification was created successfully
                {
                    await _hubContext.Clients
                        .Group($"order_{order.OrderId}")
                        .SendAsync("PaymentSuccess", order.OrderId, createdNotification); // Send Model
                    _logger.LogInformation("SignalR notification 'PaymentSuccess' sent for Order {OrderId}", orderId);
                }
            }
            catch (Exception nex) { _logger.LogError(nex, "Failed to send payment success notification/SignalR for Order {OrderId}.", orderId); }

            // The caller (e.g., Webhook Handler with UoW) would call SaveChanges here
            return true;
        }

        // --- 4. JOB CHECK EXPIRED ORDERS (Calls Notification Service) ---
        public async Task<int> CheckExpiredOrdersAsync()
        {
            _logger.LogDebug("Background Job starting: Checking expired orders...");
            int count = 0;
            // This job might run outside the request transaction scope.
            // It might need its own Unit of Work or handle saving changes itself.
            try
            {
                var expiredOrders = await _orderRepo.GetExpiredPendingOrdersAsync(); // Use own repo
                if (!expiredOrders.Any()) return 0;

                _logger.LogInformation("Background Job: Found {Count} expired orders.", expiredOrders.Count());
                foreach (var order in expiredOrders)
                {
                    // Update Order status locally
                    order.Cancel();
                    await _orderRepo.UpdateAsync(order); // Mark modified

                    // --- Call Notification Service ---
                    Notification createdNotification = null;
                    try
                    {
                        createdNotification = await _notificationService.CreateNotificationAsync( // Use method from example
                           userId: order.UserId,
                           title: "Order Expired",
                           description: $"Your booking #{order.OrderId} has expired due to non-payment.",
                           dataType: "OrderExpired",
                           dataId: order.OrderId,
                           staffId: null
                       );

                        // --- Send SignalR ---
                        if (createdNotification != null)
                        {
                            await _hubContext.Clients
                                .Group($"order_{order.OrderId}")
                                .SendAsync("OrderExpired", order.OrderId, createdNotification); // Send Model
                        }
                        _logger.LogInformation("Order {OrderId} auto-cancelled. Notifications sent.", order.OrderId);
                    }
                    catch (Exception nex) { _logger.LogError(nex, "Failed to send expiration notification/SignalR for Order {OrderId}.", order.OrderId); }
                    count++;

                    // TODO: Decide on SaveChanges strategy for background jobs.
                    // Maybe save after each order or after the loop? Requires UoW pattern or similar.
                    // For simplicity here, assume SaveChanges is handled elsewhere or implicitly.
                }
                _logger.LogInformation("Background Job finished: Processed {Count} expired orders.", count);
            }
            catch (Exception ex) { _logger.LogError(ex, "Error in CheckExpiredOrdersAsync job"); }
            return count;
        }

        // --- 5. OTHER METHODS (Orchestrating Services) ---

        //public async Task<bool> CancelOrderAsync(int orderId, int userId)
        //{
        //    _logger.LogInformation("User {UserId} attempting to cancel Order {OrderId}", userId, orderId);
        //    // Assume transaction handled outside

        //    // Use OrderRepo
        //    var order = await _orderRepo.GetByIdAsync(orderId);
        //    if (order == null) throw new ArgumentException("Order not found");
        //    if (order.UserId != userId) throw new UnauthorizedAccessException("User does not own this order.");
        //    if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed) throw new InvalidOperationException($"Cannot cancel order with status: {order.Status}");

        //    var originalStatus = order.Status;
        //    order.Cancel();
        //    await _orderRepo.UpdateAsync(order); // Mark modified

        //    // Call Payment Service for Refund
        //    if (originalStatus == OrderStatus.Confirmed)
        //    {
        //        // Assuming Payment Service has a method to handle this logic
        //        var refundSuccess = await _paymentService.RequestRefundForOrderAsync(orderId);
        //        _logger.LogInformation("Refund requested via PaymentService for Order {OrderId}. Success: {Status}", orderId, refundSuccess);
        //    }

        //    // Call Notification Service
        //    Notification createdNotification = null;
        //    try
        //    {
        //        createdNotification = await _notificationService.CreateNotificationAsync( // Use method from example
        //           userId: userId,
        //           title: "Order Cancelled",
        //           description: $"Your booking #{orderId} has been cancelled.",
        //           dataType: "OrderCancelled",
        //           dataId: orderId,
        //           staffId: null // User initiated
        //       );

        //        // Send SignalR
        //        if (createdNotification != null)
        //        {
        //            await _hubContext.Clients.Group($"order_{order.OrderId}").SendAsync("OrderCancelled", order.OrderId, createdNotification); // Send Model
        //        }
        //        _logger.LogInformation("Cancellation notifications sent for Order {OrderId}", orderId);
        //    }
        //    catch (Exception nex) { _logger.LogError(nex, "Failed to send cancellation notification/SignalR for Order {OrderId}.", orderId); }

        //    // Caller handles SaveChanges
        //    return true;
        //}

        public async Task<bool> StartRentalAsync(int orderId)
        {
            _logger.LogInformation("Attempting to start rental for Order {OrderId}", orderId);
            // Assume transaction handled outside

            // Use OrderRepo
            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order == null) throw new ArgumentException("Order not found");
            if (order.Status != OrderStatus.Confirmed) throw new InvalidOperationException($"Cannot start rental for order with status: {order.Status}");
            if (DateTime.UtcNow < order.FromDate) throw new InvalidOperationException("Rental period has not started yet.");

            order.StartRental();
            await _orderRepo.UpdateAsync(order); // Mark modified

            // Call Notification Service
            try
            {
                await _notificationService.CreateNotificationAsync( // Use method from example
                    userId: order.UserId,
                    title: "Rental Started",
                    description: $"Your rental for booking #{orderId} has started. Enjoy your trip!",
                    dataType: "RentalStarted",
                    dataId: orderId,
                    staffId: null // System event
                );
                _logger.LogInformation("Rental started notification sent for Order {OrderId}", orderId);
            }
            catch (Exception nex) { _logger.LogError(nex, "Failed to send rental started notification for Order {OrderId}.", orderId); }

            // Caller handles SaveChanges
            return true;
        }

        public async Task<bool> CompleteRentalAsync(int orderId)
        {
            _logger.LogInformation("Attempting to complete rental for Order {OrderId}", orderId);
            // Assume transaction handled outside

            // Use OrderRepo
            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order == null) throw new ArgumentException("Order not found");
            if (order.Status != OrderStatus.InProgress) throw new InvalidOperationException($"Cannot complete order with status: {order.Status}");

            order.Complete();
            await _orderRepo.UpdateAsync(order); // Mark modified

            // Call TrustScore Service
            try
            {
                // Use method from your TrustScoreService example
                await _trustScoreService.UpdateScoreOnRentalCompletionAsync(order.UserId, orderId);
                _logger.LogInformation("Trust score update requested on completion via TrustScoreService for User {UserId}, Order {OrderId}.", order.UserId, orderId);
            }
            catch (Exception tsEx) { _logger.LogError(tsEx, "Failed to update trust score on completion for User {UserId}, Order {OrderId}.", order.UserId, orderId); }


            // Call Notification Service
            try
            {
                await _notificationService.CreateNotificationAsync( // Use method from example
                    userId: order.UserId,
                    title: "Rental Completed",
                    description: $"Your rental for booking #{orderId} has completed. Thank you! Please consider leaving a review.",
                    dataType: "RentalCompleted",
                    dataId: orderId,
                    staffId: null // System event
                );
                _logger.LogInformation("Rental completed notification sent for Order {OrderId}", orderId);
            }
            catch (Exception nex) { _logger.LogError(nex, "Failed to send rental completed notification for Order {OrderId}.", orderId); }

            // Caller handles SaveChanges
            return true;
        }

        // --- Internal Helper for Overlap Check ---
        private async Task<bool> CheckForOverlappingOrdersAsync(int vehicleId, DateTime fromDate, DateTime toDate)
        {
            _logger.LogDebug("Checking overlap for Vehicle {VId} ({Start} - {End})", vehicleId, fromDate, toDate);
            var overlappingOrders = await _orderRepo.GetOverlappingOrdersAsync(
                vehicleId, fromDate, toDate,
                new[] { OrderStatus.Confirmed, OrderStatus.InProgress }
            );
            bool overlaps = overlappingOrders.Any();
            if (overlaps) _logger.LogWarning("Overlap DETECTED for Vehicle {VId}", vehicleId);
            return overlaps;
        }

        // --- Get Methods ---
        public async Task<Order?> GetOrderByIdAsync(int orderId) => await _orderRepo.GetByIdAsync(orderId);
        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId) => await _orderRepo.GetByUserIdAsync(userId);
    }
}