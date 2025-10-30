using BookingService.DTOs;
using BookingService.Models;
using BookingService.Repositories;
using BookingService.Services.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BookingService.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IPaymentService _paymentService;
        private readonly INotificationService _notificationService;
        private readonly ITrustScoreService _trustScoreService;
        private readonly IHubContext<OrderTimerHub> _hubContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly MyDbContext _context;
        private readonly ILogger<OrderService> _logger;
        private readonly OrderSettings _orderSettings;

        // ⚠️ ĐÃ XÓA: IOnlineContractService - Không còn tự động tạo contract nữa!

        public OrderService(
            IOrderRepository orderRepo,
            IPaymentService paymentService,
            INotificationService notificationService,
            ITrustScoreService trustScoreService,
            IHubContext<OrderTimerHub> hubContext,
            IUnitOfWork unitOfWork,
            MyDbContext context,
            ILogger<OrderService> logger,
            IOptions<OrderSettings> orderSettings)
        {
            _orderRepo = orderRepo ?? throw new ArgumentNullException(nameof(orderRepo));
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _trustScoreService = trustScoreService ?? throw new ArgumentNullException(nameof(trustScoreService));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _orderSettings = orderSettings?.Value ?? throw new ArgumentNullException(nameof(orderSettings));
        }

        #region Order Preview

        /// <summary>
        /// Xem trước thông tin đơn hàng trước khi tạo
        /// </summary>
        public async Task<OrderPreviewResponse> GetOrderPreviewAsync(OrderRequest request, int userId)
        {
            _logger.LogInformation(
                "Getting order preview for User {UserId}, Vehicle {VehicleId}, from {FromDate} to {ToDate}",
                userId, request.VehicleId, request.FromDate, request.ToDate);

            // 1. Validate thời gian
            var validationResult = ValidateDateRange(request.FromDate, request.ToDate);
            if (!validationResult.IsValid)
            {
                return CreatePreviewResponse(request, isAvailable: false, validationResult.Message, userId);
            }

            // 2. Kiểm tra tính khả dụng của xe
            var isAvailable = await CheckVehicleAvailabilityAsync(
                request.VehicleId,
                request.FromDate,
                request.ToDate);

            // 3. Tính toán chi phí
            var costBreakdown = await CalculateOrderCostAsync(
                userId,
                request.FromDate,
                request.ToDate,
                request.RentFeeForHour,
                request.ModelPrice);

            // 4. Trả về response
            return new OrderPreviewResponse
            {
                UserId = userId,
                VehicleId = request.VehicleId,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                TotalRentalCost = costBreakdown.RentalCost,
                DepositAmount = costBreakdown.Deposit,
                ServiceFee = costBreakdown.ServiceFee,
                TotalPaymentAmount = costBreakdown.TotalAmount,
                IsAvailable = isAvailable,
                Message = isAvailable
                    ? "Xe kh��� dụng. Vui lòng xác nhận đặt xe."
                    : "Xe đã được đặt trong khoảng thời gian này. Vui lòng chọn thời gian khác."
            };
        }

        #endregion

        #region Create Order

        /// <summary>
        /// Tạo đơn hàng mới với trạng thái Pending
        /// </summary>
        public async Task<OrderResponse> CreateOrderAsync(OrderRequest request, int userId)
        {
            _logger.LogInformation(
                "Creating order for User {UserId}, Vehicle {VehicleId}",
                userId, request.VehicleId);

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // 1. Validate đầu vào
                ValidateDateRangeOrThrow(request.FromDate, request.ToDate);

                // 2. Kiểm tra tính khả dụng (với locking để tránh race condition)
                await EnsureVehicleAvailableAsync(
                    request.VehicleId,
                    request.FromDate,
                    request.ToDate);

                // 3. Tính toán chi phí
                var costBreakdown = await CalculateOrderCostAsync(
                    userId,
                    request.FromDate,
                    request.ToDate,
                    request.RentFeeForHour,
                    request.ModelPrice);

                // 4. Lấy Trust Score
                var trustScore = await _trustScoreService.GetCurrentScoreAsync(userId);

                // 5. Tạo Order entity
                var order = CreateOrderEntity(
                    request,
                    costBreakdown,
                    trustScore,
                    userId);

                // 6. Lưu Order
                var createdOrder = await _orderRepo.CreateAsync(order);

                _logger.LogInformation("Order {OrderId} created successfully", createdOrder.OrderId);

                // 7. Tạo Payment record
                await _paymentService.CreatePaymentForOrderAsync(
                    orderId: createdOrder.OrderId,
                    amount: costBreakdown.TotalAmount,
                    paymentMethod: request.PaymentMethod ?? "VNPay");

                // 8. Tạo Notification record
                await _notificationService.CreateNotificationAsync(
                    userId: userId,
                    title: "Đơn hàng đã tạo",
                    description: $"Đơn hàng #{createdOrder.OrderId} đã được tạo. Vui lòng thanh toán trong {_orderSettings.ExpiryMinutes} phút.",
                    dataType: "OrderCreated",
                    dataId: createdOrder.OrderId,
                    staffId: null);

                // 9. Commit transaction (tất cả changes được save cùng lúc)
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation(
                    "Order {OrderId} transaction committed successfully",
                    createdOrder.OrderId);

                // 10. Trả về response
                return new OrderResponse
                {
                    OrderId = createdOrder.OrderId,
                    Status = OrderStatus.Pending.ToString(),
                    TotalAmount = costBreakdown.TotalAmount,
                    ExpiresAt = createdOrder.ExpiresAt,
                    Message = $"Đơn hàng đã được tạo. Vui lòng thanh toán trong {_orderSettings.ExpiryMinutes} phút."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for User {UserId}", userId);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        #endregion

        #region Confirm Payment

        /// <summary>
        /// ⭐ QUAN TRỌNG - ĐÃ FIX:
        /// 1. Xóa auto contract generation
        /// 2. Thêm TransactionId vào SignalR event
        /// 3. Tách SignalR ra ngoài transaction
        /// </summary>
        public async Task<bool> ConfirmPaymentAsync(
            int orderId,
            string transactionId,
            string? gatewayResponse = null)
        {
            _logger.LogInformation(
                "Confirming payment for Order {OrderId}, TransactionId: {TransactionId}",
                orderId, transactionId);

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // 1. Lấy và validate Order
                var order = await GetOrderOrThrowAsync(orderId);
                ValidateOrderStatusForPayment(order);

                // 2. Cập nhật Order status (only modify, don't save yet)
                order.Confirm();
                // ✅ FIX: Don't call UpdateAsync here - it calls SaveChangesAsync
                // which conflicts with transaction. Just mark as modified.
                _context.Entry(order).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                _logger.LogInformation("Order {OrderId} status updated to Confirmed", orderId);

                // 3. Cập nhật Payment status (also only modify, don't save yet)
                var payment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.OrderId == orderId);
                if (payment == null)
                {
                    throw new InvalidOperationException($"Payment not found for Order {orderId}");
                }

                // Idempotency check
                if (!payment.IsCompleted())
                {
                    payment.MarkAsCompleted(transactionId, gatewayResponse);
                    _context.Entry(payment).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }

                _logger.LogInformation("Payment {PaymentId} updated for Order {OrderId}", payment.PaymentId, orderId);

                // ✅ ĐÃ XÓA PHẦN TẠO CONTRACT TỰ ĐỘNG:
                // ❌ TRƯỚC ĐÂY:
                // await _contractService.GenerateAndSendContractAsync(
                //     orderId, order.UserId, order.VehicleId);
                //
                // ✅ GIẢI THÍCH:
                // - Frontend sẽ tự gọi API tạo contract sau khi nhận SignalR event
                // - Frontend có đầy đủ data (UserDto, VehicleDto, OrderPreview, TransactionId)
                // - Không cần Backend tự động tạo nữa

                // 4. Cập nhật Trust Score
                await _trustScoreService.UpdateScoreOnFirstPaymentAsync(order.UserId, orderId);

                // 5. Tạo Notification record
                await _notificationService.CreateNotificationAsync(
                    userId: order.UserId,
                    title: "Thanh toán thành công",
                    description: $"Thanh toán cho đơn hàng #{orderId} thành công. Hợp đồng sẽ được tạo ngay.",
                    dataType: "PaymentSuccess",
                    dataId: orderId,
                    staffId: null);

                // 6. Commit transaction
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation(
                    "Payment confirmed successfully for Order {OrderId}",
                    orderId);

                // ✅ ĐÃ FIX: Gửi SignalR event SAU KHI COMMIT
                // ✅ ĐÃ FIX: Thêm TransactionId vào event payload
                try
                {
                    await NotifyPaymentSuccessViaSignalR(orderId, order.UserId, transactionId);
                }
                catch (Exception ex)
                {
                    // SignalR fail không ảnh hưởng logic chính
                    _logger.LogError(ex,
                        "Failed to send SignalR notification for Order {OrderId}", orderId);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming payment for Order {OrderId}", orderId);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        #endregion

        #region Background Job - Check Expired Orders

        /// <summary>
        /// Background job: Kiểm tra và hủy các đơn hàng Pending đã hết hạn
        /// </summary>
        public async Task<int> CheckExpiredOrdersAsync()
        {
            _logger.LogInformation("Starting expired orders check");

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var expiredOrders = await _orderRepo.GetExpiredPendingOrdersAsync();
                int processedCount = 0;

                foreach (var order in expiredOrders)
                {
                    try
                    {
                        await ProcessExpiredOrderAsync(order);
                        processedCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Error processing expired Order {OrderId}",
                            order.OrderId);
                        // Continue with other orders
                    }
                }

                await _unitOfWork.CommitTransactionAsync();

                // ✅ FIX: Gửi SignalR SAU KHI COMMIT
                foreach (var order in expiredOrders)
                {
                    try
                    {
                        await _hubContext.Clients.User(order.UserId.ToString())
                            .SendAsync("OrderExpired", new
                            {
                                OrderId = order.OrderId,
                                Message = "Đơn hàng đã hết hạn!"
                            });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Failed to send SignalR for expired Order {OrderId}", order.OrderId);
                    }
                }

                _logger.LogInformation(
                    "Expired orders check completed. Processed: {Count}",
                    processedCount);

                return processedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during expired orders check");
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        private async Task ProcessExpiredOrderAsync(Order order)
        {
            _logger.LogInformation("Processing expired Order {OrderId}", order.OrderId);

            // Cancel order
            order.Cancel();
            await _orderRepo.UpdateAsync(order);

            // Send notification (insert record vào DB)
            await _notificationService.CreateNotificationAsync(
                userId: order.UserId,
                title: "Đơn hàng hết hạn",
                description: $"Đơn hàng #{order.OrderId} đã hết hạn do chưa thanh toán.",
                dataType: "OrderExpired",
                dataId: order.OrderId,
                staffId: null);

            _logger.LogInformation("Order {OrderId} marked as expired", order.OrderId);
        }

        #endregion

        #region Start & Complete Rental

        /// <summary>
        /// Bắt đầu chuyến thuê xe
        /// </summary>
        public async Task<bool> StartRentalAsync(int orderId)
        {
            _logger.LogInformation("Starting rental for Order {OrderId}", orderId);

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var order = await GetOrderOrThrowAsync(orderId);

                if (order.Status != OrderStatus.Confirmed)
                {
                    throw new InvalidOperationException(
                        $"Order {orderId} must be in Confirmed status to start rental. Current status: {order.Status}");
                }

                order.StartRental();
                await _orderRepo.UpdateAsync(order);

                await _notificationService.CreateNotificationAsync(
                    userId: order.UserId,
                    title: "Chuyến thuê đã bắt đầu",
                    description: $"Chuyến thuê xe #{orderId} đã bắt đầu.",
                    dataType: "RentalStarted",
                    dataId: orderId,
                    staffId: null);

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Rental started successfully for Order {OrderId}", orderId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting rental for Order {OrderId}", orderId);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        /// <summary>
        /// Hoàn thành chuyến thuê xe
        /// </summary>
        public async Task<bool> CompleteRentalAsync(int orderId)
        {
            _logger.LogInformation("Completing rental for Order {OrderId}", orderId);

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var order = await GetOrderOrThrowAsync(orderId);

                if (order.Status != OrderStatus.InProgress)
                {
                    throw new InvalidOperationException(
                        $"Order {orderId} must be in InProgress status to complete. Current status: {order.Status}");
                }

                order.Complete();
                await _orderRepo.UpdateAsync(order);

                // Update Trust Score
                await _trustScoreService.UpdateScoreOnRentalCompletionAsync(order.UserId, orderId);

                await _notificationService.CreateNotificationAsync(
                    userId: order.UserId,
                    title: "Chuyến thuê hoàn thành",
                    description: $"Chuyến thuê xe #{orderId} đã hoàn thành. Cảm ơn bạn đã sử dụng dịch vụ!",
                    dataType: "RentalCompleted",
                    dataId: orderId,
                    staffId: null);

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Rental completed successfully for Order {OrderId}", orderId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing rental for Order {OrderId}", orderId);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        #endregion

        #region Query Methods

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _orderRepo.GetByIdAsync(orderId);
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId)
        {
            return await _orderRepo.GetByUserIdAsync(userId);
        }

        #endregion

        #region Private Helper Methods

        // ===== Validation =====

        private (bool IsValid, string Message) ValidateDateRange(DateTime fromDate, DateTime toDate)
        {
            if (fromDate >= toDate)
            {
                return (false, "Thời gian thuê không hợp lệ. FromDate phải nhỏ hơn ToDate.");
            }

            if (fromDate < DateTime.UtcNow)
            {
                return (false, "Không thể đặt xe trong quá khứ.");
            }

            return (true, string.Empty);
        }

        private void ValidateDateRangeOrThrow(DateTime fromDate, DateTime toDate)
        {
            var result = ValidateDateRange(fromDate, toDate);
            if (!result.IsValid)
            {
                throw new InvalidOperationException(result.Message);
            }
        }

        private void ValidateOrderStatusForPayment(Order order)
        {
            if (order.Status != OrderStatus.Pending)
            {
                throw new InvalidOperationException(
                    $"Order {order.OrderId} is not in Pending status. Current status: {order.Status}");
            }
        }

        // ===== Availability Checks =====

        private async Task<bool> CheckVehicleAvailabilityAsync(
            int vehicleId,
            DateTime fromDate,
            DateTime toDate)
        {
            var statuses = new[] { OrderStatus.Confirmed, OrderStatus.InProgress, OrderStatus.Pending };
            var overlappingOrders = await _orderRepo.GetOverlappingOrdersAsync(
                vehicleId,
                fromDate,
                toDate,
                statuses);

            return !overlappingOrders.Any();
        }

        private async Task EnsureVehicleAvailableAsync(
            int vehicleId,
            DateTime fromDate,
            DateTime toDate)
        {
            var isAvailable = await _orderRepo.IsVehicleAvailableAsync(
                vehicleId,
                fromDate,
                toDate);

            if (!isAvailable)
            {
                throw new InvalidOperationException(
                    $"Vehicle {vehicleId} is not available from {fromDate:yyyy-MM-dd HH:mm} to {toDate:yyyy-MM-dd HH:mm}");
            }
        }

        // ===== Cost Calculation =====

        private async Task<OrderCostBreakdown> CalculateOrderCostAsync(
            int userId,
            DateTime fromDate,
            DateTime toDate,
            decimal hourlyRate,
            decimal vehiclePrice)
        {
            var totalHours = (decimal)(toDate - fromDate).TotalHours;
            var rentalCost = totalHours * hourlyRate;
            var deposit = vehiclePrice * _orderSettings.DepositPercentage;

            // Check if user has completed orders (waive service fee)
            var hasCompletedOrder = await _orderRepo.HasCompletedOrderAsync(userId);
            var serviceFee = hasCompletedOrder ? 0m : _orderSettings.ServiceFee;

            var totalAmount = rentalCost + deposit + serviceFee;

            return new OrderCostBreakdown
            {
                RentalCost = rentalCost,
                Deposit = deposit,
                ServiceFee = serviceFee,
                TotalAmount = totalAmount
            };
        }

        // ===== Entity Creation =====

        private Order CreateOrderEntity(
            OrderRequest request,
            OrderCostBreakdown costBreakdown,
            int trustScore, int userId)
        {
            return new Order(
                userId: userId,
                vehicleId: request.VehicleId,
                fromDate: request.FromDate,
                toDate: request.ToDate,
                hourlyRate: request.RentFeeForHour,
                totalCost: costBreakdown.TotalAmount,
                depositAmount: costBreakdown.Deposit,
                trustScore: trustScore,
                expiresAt: DateTime.UtcNow.AddMinutes(_orderSettings.ExpiryMinutes));
        }

        // ===== Repository Helpers =====

        private async Task<Order> GetOrderOrThrowAsync(int orderId)
        {
            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order == null)
            {
                throw new InvalidOperationException($"Order {orderId} not found");
            }
            return order;
        }

        // ===== Response Creation =====

        private OrderPreviewResponse CreatePreviewResponse(
            OrderRequest request,
            bool isAvailable,
            string message,
            int userId)
        {
            return new OrderPreviewResponse
            {
                UserId = userId,
                VehicleId = request.VehicleId,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                IsAvailable = isAvailable,
                Message = message
            };
        }

        // ===== SignalR =====

        /// <summary>
        /// ✅ ĐÃ FIX: Thêm TransactionId vào event
        /// </summary>
        private async Task NotifyPaymentSuccessViaSignalR(
            int orderId,
            int userId,
            string transactionId)
        {
            await _hubContext.Clients.User(userId.ToString())
                .SendAsync("PaymentSuccess", new
                {
                    OrderId = orderId,
                    TransactionId = transactionId, // ✅ THÊM FIELD NÀY!
                    Message = "Thanh toán thành công!"
                });

            _logger.LogInformation(
                "SignalR PaymentSuccess sent to User {UserId} for Order {OrderId}, TransactionId: {TransactionId}",
                userId, orderId, transactionId);
        }



        #endregion
    }

    // ===== Helper Classes =====

    internal class OrderCostBreakdown
    {
        public decimal RentalCost { get; set; }
        public decimal Deposit { get; set; }
        public decimal ServiceFee { get; set; }
        public decimal TotalAmount { get; set; }
    }

    // ===== Settings Class (nên định nghĩa riêng file) =====

    public class OrderSettings
    {
        public decimal DepositPercentage { get; set; } = 0.3m; // 30%
        public decimal ServiceFee { get; set; } = 50000m;
        public int ExpiryMinutes { get; set; } = 30;
    }
}
