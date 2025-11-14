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
        private readonly IImageStorageService _imageStorageService;
        private readonly IVehicleCheckInRepository _vehicleCheckInRepo;
        private readonly IVehicleReturnRepository _vehicleReturnRepo;
        private readonly ISettlementService _settlementService;

        // ⚠️ ĐÃ XÓA: IOnlineContractService - Không còn tự động tạo contract nữa!

        private readonly IServiceProvider _serviceProvider;

        public OrderService(
            IOrderRepository orderRepo,
            IPaymentService paymentService,
            INotificationService notificationService,
            ITrustScoreService trustScoreService,
            IHubContext<OrderTimerHub> hubContext,
            IUnitOfWork unitOfWork,
            MyDbContext context,
            ILogger<OrderService> logger,
            IOptions<OrderSettings> orderSettings,
            IImageStorageService imageStorageService,
            IVehicleCheckInRepository vehicleCheckInRepo,
            IVehicleReturnRepository vehicleReturnRepo,
            ISettlementService settlementService,
            IServiceProvider serviceProvider)
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
            _imageStorageService = imageStorageService ?? throw new ArgumentNullException(nameof(imageStorageService));
            _vehicleCheckInRepo = vehicleCheckInRepo ?? throw new ArgumentNullException(nameof(vehicleCheckInRepo));
            _vehicleReturnRepo = vehicleReturnRepo ?? throw new ArgumentNullException(nameof(vehicleReturnRepo));
            _settlementService = settlementService ?? throw new ArgumentNullException(nameof(settlementService));
            _serviceProvider = serviceProvider;
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
        /// Bắt đầu chuyến thuê xe (với hình ảnh xác nhận xe)
        /// </summary>
        public async Task<bool> StartRentalAsync(int orderId, List<IFormFile> images, int confirmedBy, VehicleCheckInRequest request)
        {
            _logger.LogInformation("Starting rental for Order {OrderId} with vehicle images", orderId);

            try
            {
                // Validate images
                if (images == null || images.Count == 0)
                {
                    throw new InvalidOperationException("Phải có ít nhất một ảnh xe để bắt đầu thuê.");
                }

                await _unitOfWork.BeginTransactionAsync();

                var order = await GetOrderOrThrowAsync(orderId);

                if (order.Status != OrderStatus.Confirmed)
                {
                    throw new InvalidOperationException(
                        $"Order {orderId} must be in Confirmed status to start rental. Current status: {order.Status}");
                }

                // Save images
                var imageUrls = await _imageStorageService.SaveImagesAsync(images, "vehicle-checkin");

                if (string.IsNullOrEmpty(imageUrls))
                {
                    throw new InvalidOperationException("Không thể lưu ảnh xe. Vui lòng thử lại.");
                }

                // Create vehicle check-in record
                var checkIn = new VehicleCheckIn
                {
                    OrderId = orderId,
                    CheckInTime = DateTime.UtcNow,
                    OdometerReading = request.OdometerReading,
                    FuelLevel = request.FuelLevel,
                    ImageUrls = imageUrls,
                    Notes = request.Notes,
                    ConfirmedBy = confirmedBy
                };

                await _vehicleCheckInRepo.CreateAsync(checkIn);

                // Start the rental (update order status)
                order.StartRental();
                await _orderRepo.UpdateAsync(order);

                await _notificationService.CreateNotificationAsync(
                    userId: order.UserId,
                    title: "Chuyến thuê đã bắt đầu",
                    description: $"Chuyến thuê xe #{orderId} đã bắt đầu. Ảnh xe đã được lưu.",
                    dataType: "RentalStarted",
                    dataId: orderId,
                    staffId: null);

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Rental started successfully for Order {OrderId} with {ImageCount} images", orderId, images.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting rental for Order {OrderId}", orderId);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        private async Task SchedulePayOSRefundAsync(Order order, bool hasDamage)
        {
            if (order == null)
            {
                return;
            }

            if (hasDamage)
            {
                _logger.LogInformation("Order {OrderId} reported damage. Skipping automatic PayOS refund.", order.OrderId);
                return;
            }

            Payment? payment;
            try
            {
                payment = await _paymentService.GetPaymentByOrderIdAsync(order.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to retrieve payment for Order {OrderId}. Skipping refund.", order.OrderId);
                return;
            }

            if (payment == null || !string.Equals(payment.PaymentMethod, "PayOS", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(payment.TransactionId))
            {
                _logger.LogWarning("Order {OrderId} has no PayOS transaction id. Cannot process refund.", order.OrderId);
                return;
            }

            if (order.DepositAmount <= 0)
            {
                _logger.LogInformation("Order {OrderId} deposit amount is {Deposit}. No refund necessary.", order.OrderId, order.DepositAmount);
                return;
            }

            try
            {
                var payos = _serviceProvider?.GetService<IPayOSService>();
                if (payos == null)
                {
                    _logger.LogWarning("IPayOSService not resolved from provider. Refund skipped for Order {OrderId}.", order.OrderId);
                    return;
                }

                try
                {
                    _logger.LogInformation("Attempting PayOS refund for Order {OrderId} - Transaction {TransactionId}, Amount {Amount}",
                        order.OrderId, payment.TransactionId, order.DepositAmount);

                    var refundResult = await payos.RefundDepositAsync(payment.TransactionId!, order.DepositAmount,
                        $"Refund deposit for Order #{order.OrderId}");

                    if (!refundResult.Success)
                    {
                        _logger.LogWarning("PayOS refund failed for Order {OrderId}: {Error}", order.OrderId, refundResult.Error ?? "unknown error");
                        return;
                    }

                    _logger.LogInformation("PayOS refund success for Order {OrderId}, RefundId {RefundId}",
                        order.OrderId, refundResult.RefundId);

                    try
                    {
                        await _paymentService.MarkDepositRefundedAsync(payment.PaymentId, refundResult.RefundId, DateTime.UtcNow, order.DepositAmount, null);
                    }
                    catch (Exception updateEx)
                    {
                        _logger.LogWarning(updateEx, "Failed to update payment refund metadata for Order {OrderId}", order.OrderId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "PayOS refund background task error for Order {OrderId}", order.OrderId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to schedule PayOS refund for Order {OrderId}", order.OrderId);
            }
        }

        /// <summary>
        /// Hoàn thành chuyến thuê xe (với hình ảnh xác nhận trả xe)
        /// </summary>
        public async Task<bool> CompleteRentalAsync(int orderId, List<IFormFile> images, int confirmedBy, VehicleReturnRequest request)
        {
            _logger.LogInformation("Completing rental for Order {OrderId} with vehicle return images", orderId);

            try
            {
                // Validate images
                if (images == null || images.Count == 0)
                {
                    throw new InvalidOperationException("Phải có ít nhất một ảnh xe để hoàn thành thuê.");
                }

                await _unitOfWork.BeginTransactionAsync();

                var order = await GetOrderOrThrowAsync(orderId);

                if (order.Status != OrderStatus.InProgress)
                {
                    throw new InvalidOperationException(
                        $"Order {orderId} must be in InProgress status to complete. Current status: {order.Status}");
                }

                // Save return images
                var imageUrls = await _imageStorageService.SaveImagesAsync(images, "vehicle-return");

                if (string.IsNullOrEmpty(imageUrls))
                {
                    throw new InvalidOperationException("Không thể lưu ảnh xe. Vui lòng thử lại.");
                }

                // Create vehicle return record
                var vehicleReturn = new VehicleReturn
                {
                    OrderId = orderId,
                    ReturnTime = DateTime.UtcNow,
                    OdometerReading = request.OdometerReading,
                    FuelLevel = request.FuelLevel,
                    ImageUrls = imageUrls,
                    ConditionNotes = request.ConditionNotes,
                    HasDamage = request.HasDamage,
                    DamageDescription = request.DamageDescription,
                    // DamageCharge removed - customers should not set monetary charges
                    // Staff will add actual damage charges via POST /api/settlement/{orderId}/damage
                    DamageCharge = 0, // Default to 0 until staff assessment
                    ConfirmedBy = confirmedBy
                };

                await _vehicleReturnRepo.CreateAsync(vehicleReturn);

                // Create settlement automatically with actual return time
                try
                {
                    var actualReturnTime = vehicleReturn.ReturnTime;
                    await _settlementService.CreateSettlementAsync(orderId, actualReturnTime);
                    _logger.LogInformation("Settlement created automatically for Order {OrderId} at {ReturnTime}", orderId, actualReturnTime);
                    
                    // Auto-finalize settlement if no damage (allows immediate refund processing)
                    if (!vehicleReturn.HasDamage)
                    {
                        try
                        {
                            await _settlementService.FinalizeSettlementAsync(orderId);
                            _logger.LogInformation("Settlement auto-finalized for Order {OrderId} (no damage)", orderId);
                        }
                        catch (Exception finalizeEx)
                        {
                            // Log but don't fail - settlement can be finalized manually later
                            _logger.LogWarning(finalizeEx, "Failed to auto-finalize settlement for Order {OrderId}. Can be finalized manually later.", orderId);
                        }
                    }
                }
                catch (Exception settlementEx)
                {
                    // Log but don't fail the rental completion if settlement creation fails
                    // Settlement can be created manually later if needed
                    _logger.LogWarning(settlementEx, "Failed to create settlement for Order {OrderId}. Settlement can be created manually later.", orderId);
                }

                // Complete the order (update status)
                order.Complete();
                await _orderRepo.UpdateAsync(order);

                // Update Trust Score (+10 bonus for completion)
                await _trustScoreService.UpdateScoreOnRentalCompletionAsync(order.UserId, orderId);

                var notificationMessage = vehicleReturn.HasDamage
                    ? $"Chuyến thuê xe #{orderId} đã hoàn thành. Có phát hiện hư hỏng, vui lòng chờ xử lý thanh toán."
                    : $"Chuyến thuê xe #{orderId} đã hoàn thành. Cảm ơn bạn đã sử dụng dịch vụ!";

                await _notificationService.CreateNotificationAsync(
                    userId: order.UserId,
                    title: "Chuyến thuê hoàn thành",
                    description: notificationMessage,
                    dataType: "RentalCompleted",
                    dataId: orderId,
                    staffId: null);

                await _unitOfWork.CommitTransactionAsync();

                // Attempt deposit refund for PayOS payments (non-blocking)
                _ = SchedulePayOSRefundAsync(order, vehicleReturn.HasDamage);

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

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            _logger.LogInformation("Getting all orders");
            return await _orderRepo.GetAllAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _orderRepo.GetByIdAsync(orderId);
        }

        public async Task<Order?> GetOrderByIdWithDetailsAsync(int orderId)
        {
            return await _orderRepo.GetByIdWithDetailsAsync(orderId);
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId)
        {
            return await _orderRepo.GetByUserIdAsync(userId);
        }

        public async Task<PeakHoursReportResponse> GetPeakHoursReportAsync()
        {
            _logger.LogInformation("Getting peak hours report");

            try
            {
                var hourlyCounts = await _orderRepo.GetOrderCountByHourAsync();
                var topPeakHours = await _orderRepo.GetTopPeakHoursAsync(3);
                var totalOrders = hourlyCounts.Values.Sum();

                var hourlyData = new List<HourlyOrderCount>();
                for (int hour = 0; hour < 24; hour++)
                {
                    var count = hourlyCounts.ContainsKey(hour) ? hourlyCounts[hour] : 0;
                    var percentage = totalOrders > 0 ? (count * 100.0 / totalOrders) : 0.0;
                    var isPeakHour = topPeakHours.Contains(hour);

                    hourlyData.Add(new HourlyOrderCount
                    {
                        Hour = hour,
                        TimeSlot = $"{hour:D2}:00 - {(hour + 1):D2}:00",
                        OrderCount = count,
                        Percentage = Math.Round(percentage, 2),
                        IsPeakHour = isPeakHour
                    });
                }

                return new PeakHoursReportResponse
                {
                    TotalOrders = totalOrders,
                    HourlyData = hourlyData,
                    TopPeakHours = topPeakHours,
                    GeneratedAt = DateTime.UtcNow.ToString("o")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting peak hours report");
                throw;
            }
        }

        public async Task UpdateOrderAsync(Order order)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            _logger.LogInformation("Updating Order {OrderId}", order.OrderId);

            try
            {
                var success = await _orderRepo.UpdateAsync(order);
                if (!success)
                {
                    throw new InvalidOperationException($"Failed to update Order {order.OrderId}");
                }

                _logger.LogInformation("Order {OrderId} updated successfully", order.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Order {OrderId}", order.OrderId);
                throw;
            }
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

            // Tính tiền cọc ban đầu (30% của giá xe)
            var baseDeposit = vehiclePrice * _orderSettings.DepositPercentage;

            // Check if user has completed orders (waive service fee)
            var hasCompletedOrder = await _orderRepo.HasCompletedOrderAsync(userId);
            var serviceFee = hasCompletedOrder ? 0m : _orderSettings.ServiceFee;

            // Tính tiền cọc dựa trên TrustScore nếu user có đơn hàng
            var deposit = baseDeposit;
            if (hasCompletedOrder)
            {
                var trustScore = await _trustScoreService.GetCurrentScoreAsync(userId);

                if (trustScore >= 1000)
                {
                    // Miễn cọc nếu điểm >= 1000
                    deposit = 0m;
                }
                else if (trustScore >= 500)
                {
                    // Giảm 50% cọc nếu điểm >= 500
                    deposit = baseDeposit * 0.5m;
                }
                // Nếu điểm < 500 thì giữ nguyên tiền cọc
            }

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
