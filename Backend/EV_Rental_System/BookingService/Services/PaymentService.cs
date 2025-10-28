using BookingSerivce.DTOs;
using BookingService.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BookingService.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly MyDbContext _context;
        private readonly ILogger<PaymentService> _logger;
        private readonly IVNPayService _vnPayService;
        private readonly PaymentSettings _settings;

        public PaymentService(
            MyDbContext context,
            ILogger<PaymentService> logger,
            IVNPayService vnPayService,
            IConfiguration configuration)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _vnPayService = vnPayService ?? throw new ArgumentNullException(nameof(vnPayService));

            _settings = new PaymentSettings
            {
                ExpirationMinutes = configuration.GetValue<int>("PaymentSettings:PaymentExpirationMinutes", 15),
                BatchSize = configuration.GetValue<int>("PaymentSettings:BatchSize", 10),
                BatchDelayMs = configuration.GetValue<int>("PaymentSettings:BatchDelayMs", 100)
            };
        }

        // ============== CORE PAYMENT OPERATIONS ==============

        public async Task<Payment> CreatePaymentForOrderAsync(
            int orderId,
            decimal amount,
            string paymentMethod)
        {
            ValidatePaymentCreation(orderId, amount, paymentMethod);

            var payment = new Payment(orderId, amount, paymentMethod, _settings.ExpirationMinutes);

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Created Payment {PaymentId} for Order {OrderId}, Amount: {Amount}, ExpiresAt: {ExpiresAt}",
                payment.PaymentId, orderId, amount, payment.ExpiresAt);

            return payment;
        }

        public async Task<bool> MarkPaymentCompletedAsync(
            int orderId,
            string transactionId,
            string? gatewayResponse)
        {
            ValidateTransactionId(transactionId);

            return await ExecuteInTransactionAsync(async () =>
            {
                var payment = await GetPendingPaymentByOrderIdAsync(orderId);
                if (payment == null)
                {
                    _logger.LogWarning("Pending Payment not found for Order {OrderId}", orderId);
                    return false;
                }

                // Idempotency: already completed
                if (payment.IsCompleted())
                {
                    _logger.LogInformation(
                        "Payment {PaymentId} already completed (idempotency check)",
                        payment.PaymentId);
                    return true;
                }

                // Mark payment as completed
                payment.MarkAsCompleted(transactionId, gatewayResponse);

                // Update order status
                var order = await GetOrderByIdAsync(orderId);
                order.Confirm();

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "✅ Payment {PaymentId} completed - Order {OrderId} confirmed, TxnId: {TransactionId}",
                    payment.PaymentId, orderId, transactionId);

                return true;
            },
            $"complete payment for Order {orderId}");
        }

        public async Task<bool> MarkPaymentFailedAsync(
            int orderId,
            string? gatewayResponse)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                var payment = await GetPendingPaymentByOrderIdAsync(orderId);
                if (payment == null)
                {
                    _logger.LogWarning("Pending Payment not found for Order {OrderId}", orderId);
                    return false;
                }

                payment.MarkAsFailed(gatewayResponse);

                var order = await GetOrderByIdAsync(orderId);
                order.Cancel();

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "❌ Payment {PaymentId} failed - Order {OrderId} cancelled",
                    payment.PaymentId, orderId);

                return true;
            },
            $"mark payment as failed for Order {orderId}");
        }

        public async Task<bool> MarkPaymentCancelledAsync(
            int orderId,
            string? reason)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                var payment = await GetPendingPaymentByOrderIdAsync(orderId);
                if (payment == null)
                {
                    _logger.LogWarning("Pending Payment not found for Order {OrderId}", orderId);
                    return false;
                }

                payment.MarkAsCancelled(reason ?? "Payment cancelled by system");

                var order = await GetOrderByIdAsync(orderId);
                order.Cancel();

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "⚠️ Payment {PaymentId} cancelled - Order {OrderId}, Reason: {Reason}",
                    payment.PaymentId, orderId, reason);

                return true;
            },
            $"cancel payment for Order {orderId}");
        }

        // ============== QUERY METHODS ==============

        public async Task<Payment?> GetPaymentByOrderIdAsync(int orderId)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.OrderId == orderId);
        }

        public async Task<Payment?> GetPaymentByTransactionIdAsync(string transactionId)
        {
            if (string.IsNullOrWhiteSpace(transactionId))
                return null;

            return await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.TransactionId == transactionId);
        }

        public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync()
        {
            return await _context.Payments
                .Where(p => p.Status == PaymentStatus.Pending)
                .Include(p => p.Order)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetExpiredPendingPaymentsAsync(
            int? expirationMinutes = null)
        {
            var expiration = expirationMinutes ?? _settings.ExpirationMinutes;
            var now = DateTime.UtcNow;

            var expiredPayments = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Pending
                    && p.ExpiresAt.HasValue
                    && p.ExpiresAt.Value <= now)
                .Include(p => p.Order)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();

            _logger.LogInformation(
                "Found {Count} expired pending payments",
                expiredPayments.Count);

            return expiredPayments;
        }

        // ============== BACKGROUND JOB OPERATIONS ==============

        public async Task<bool> CancelPaymentDueToTimeoutAsync(
            int paymentId,
            string reason = "Payment timeout")
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                var payment = await _context.Payments
                    .Include(p => p.Order)
                    .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

                if (payment == null)
                {
                    _logger.LogWarning("Payment {PaymentId} not found", paymentId);
                    return false;
                }

                if (!payment.CanBeCancelled())
                {
                    _logger.LogDebug(
                        "Payment {PaymentId} cannot be cancelled (Status: {Status})",
                        paymentId, payment.Status);
                    return false;
                }

                payment.MarkAsCancelled(reason);

                var order = payment.Order ?? await GetOrderByIdAsync(payment.OrderId);
                order.Cancel();

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "⏱️ Payment {PaymentId} timed out - Order {OrderId} cancelled. Reason: {Reason}",
                    paymentId, payment.OrderId, reason);

                return true;
            },
            $"cancel payment {paymentId} due to timeout");
        }

        public async Task<int> ProcessExpiredPaymentsAsync(int? expirationMinutes = null)
        {
            var expiration = expirationMinutes ?? _settings.ExpirationMinutes;

            _logger.LogInformation(
                "🔄 Starting expired payments processing (expiration: {Minutes} min)",
                expiration);

            var expiredPayments = await GetExpiredPendingPaymentsAsync(expiration);
            if (!expiredPayments.Any())
            {
                _logger.LogInformation("No expired payments to process");
                return 0;
            }

            var result = await ProcessPaymentBatchAsync(
                expiredPayments,
                payment => CancelPaymentDueToTimeoutAsync(
                    payment.PaymentId,
                    $"Payment expired after {expiration} minutes"));

            _logger.LogInformation(
                "✅ Processed {Total} expired payments: {Success} cancelled, {Failed} errors",
                expiredPayments.Count(), result.SuccessCount, result.Errors.Count);

            return result.SuccessCount;
        }

        // ============== GATEWAY SYNC ==============

        public async Task<bool> SyncPaymentStatusFromGatewayAsync(int paymentId)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                var payment = await _context.Payments
                    .Include(p => p.Order)
                    .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

                if (payment == null)
                {
                    _logger.LogWarning("Payment {PaymentId} not found for sync", paymentId);
                    return false;
                }

                if (!payment.IsPending())
                {
                    _logger.LogDebug(
                        "Payment {PaymentId} is not pending (Status: {Status}), skip sync",
                        paymentId, payment.Status);
                    return false;
                }

                // Get TxnRef (should be stored in payment)
                var txnRef = GetTxnRefFromPayment(payment);
                if (string.IsNullOrEmpty(txnRef))
                {
                    _logger.LogWarning(
                        "Cannot determine TxnRef for Payment {PaymentId}",
                        paymentId);
                    return false;
                }

                _logger.LogInformation(
                    "🔍 Syncing Payment {PaymentId} with VNPay, TxnRef: {TxnRef}",
                    paymentId, txnRef);

                // Query VNPay
                var queryResult = await _vnPayService.QueryTransactionAsync(
                    txnRef,
                    payment.CreatedAt);

                if (queryResult == null)
                {
                    _logger.LogError(
                        "Failed to query VNPay for Payment {PaymentId}",
                        paymentId);
                    return false;
                }

                // Process query result
                return await ProcessGatewayQueryResultAsync(payment, queryResult);
            },
            $"sync payment {paymentId} from gateway");
        }

        public async Task<int> SyncAllPendingPaymentsAsync(int minAgeMinutes = 5)
        {
            _logger.LogInformation(
                "🔄 Starting sync for pending payments (min age: {Minutes} min)",
                minAgeMinutes);

            var pendingPayments = (await GetPendingPaymentsAsync())
                .Where(p => (DateTime.UtcNow - p.CreatedAt).TotalMinutes >= minAgeMinutes)
                .ToList();

            if (!pendingPayments.Any())
            {
                _logger.LogInformation("No pending payments to sync");
                return 0;
            }

            var result = await ProcessPaymentBatchAsync(
                pendingPayments,
                payment => SyncPaymentStatusFromGatewayAsync(payment.PaymentId),
                delayMs: 500); // Longer delay to avoid rate limiting

            _logger.LogInformation(
                "✅ Synced {Success}/{Total} pending payments",
                result.SuccessCount, pendingPayments.Count);

            return result.SuccessCount;
        }

        // ============== OWNERSHIP VALIDATION ==============

        public async Task<bool> ValidateOrderOwnershipAsync(int orderId, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId cannot be null or empty", nameof(userId));

            try
            {
                var order = await _context.Orders
                    .AsNoTracking()
                    .Where(o => o.OrderId == orderId)
                    .Select(o => new { o.OrderId, o.UserId })
                    .FirstOrDefaultAsync();

                if (order == null)
                {
                    _logger.LogWarning("Order {OrderId} not found", orderId);
                    return false;
                }

                var isOwner = order.UserId.ToString() == userId;

                if (!isOwner)
                {
                    _logger.LogWarning(
                        "🚫 User {UserId} attempted to access Order {OrderId} (Owner: {OwnerId})",
                        userId, orderId, order.UserId);
                }

                return isOwner;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating ownership for Order {OrderId}", orderId);
                return false;
            }
        }

        // ============== ADMIN OPERATIONS ==============

        public async Task<bool> DeletePaymentAsync(int paymentId)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
                return false;

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            _logger.LogWarning("🗑️ Deleted Payment {PaymentId}", paymentId);
            return true;
        }

        // ============== PRIVATE HELPER METHODS ==============

        private async Task<Payment> GetPendingPaymentByOrderIdAsync(int orderId)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId && p.Status == PaymentStatus.Pending);

            if (payment == null)
                throw new InvalidOperationException($"Pending payment not found for Order {orderId}");

            return payment;
        }

        private async Task<Order> GetOrderByIdAsync(int orderId)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
                throw new InvalidOperationException($"Order {orderId} not found");

            return order;
        }

        private async Task<T> ExecuteInTransactionAsync<T>(
            Func<Task<T>> action,
            string operationDescription)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var result = await action();
                await transaction.CommitAsync();
                return result;
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning(ex, "Cannot {Operation}: {Message}",
                    operationDescription, ex.Message);
                return default!;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error during {Operation}", operationDescription);
                throw;
            }
        }

        private async Task<BatchProcessResult> ProcessPaymentBatchAsync(
            IEnumerable<Payment> payments,
            Func<Payment, Task<bool>> processor,
            int? delayMs = null)
        {
            var delay = delayMs ?? _settings.BatchDelayMs;
            var result = new BatchProcessResult();

            var batches = payments
                .Select((payment, index) => new { payment, index })
                .GroupBy(x => x.index / _settings.BatchSize)
                .Select(g => g.Select(x => x.payment).ToList());

            foreach (var batch in batches)
            {
                foreach (var payment in batch)
                {
                    try
                    {
                        var success = await processor(payment);
                        if (success)
                        {
                            result.SuccessCount++;
                        }
                        else
                        {
                            result.Errors.Add($"Failed to process Payment {payment.PaymentId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing Payment {PaymentId}", payment.PaymentId);
                        result.Errors.Add($"Exception for Payment {payment.PaymentId}: {ex.Message}");
                    }
                }

                // Delay between batches
                if (batches.Count() > 1)
                {
                    await Task.Delay(delay);
                }
            }

            if (result.Errors.Any())
            {
                _logger.LogWarning("Batch processing completed with {Count} errors", result.Errors.Count);
            }

            return result;
        }

        private async Task<bool> ProcessGatewayQueryResultAsync(
            Payment payment,
            VNPayQueryResponse queryResult)
        {
            // Query failed
            if (!queryResult.IsSuccess)
            {
                _logger.LogWarning(
                    "VNPay query failed - Payment: {PaymentId}, Code: {Code}, Message: {Message}",
                    payment.PaymentId, queryResult.ResponseCode, queryResult.Message);

                // Transaction not found and expired -> cancel
                if (queryResult.IsTransactionNotFound && payment.IsExpired)
                {
                    payment.MarkAsCancelled("Transaction not found in gateway after expiration");

                    var order = payment.Order ?? await GetOrderByIdAsync(payment.OrderId);
                    order.Cancel();

                    await _context.SaveChangesAsync();

                    _logger.LogInformation(
                        "Payment {PaymentId} cancelled - not found in gateway and expired",
                        payment.PaymentId);

                    return true;
                }

                return false;
            }

            // Transaction successful
            if (queryResult.IsTransactionSuccess)
            {
                // Validate amount
                if (queryResult.Amount != payment.Amount)
                {
                    _logger.LogError(
                        "❌ Amount mismatch - Payment: {PaymentId}, Expected: {Expected}, Gateway: {Gateway}",
                        payment.PaymentId, payment.Amount, queryResult.Amount);

                    var errorResponse = JsonSerializer.Serialize(new
                    {
                        error = "Amount mismatch",
                        expected = payment.Amount,
                        actual = queryResult.Amount,
                        gatewayResponse = queryResult
                    });

                    payment.MarkAsFailed(errorResponse);
                    await _context.SaveChangesAsync();

                    return false;
                }

                // Mark as completed
                var gatewayResponse = JsonSerializer.Serialize(queryResult);
                payment.MarkAsCompleted(queryResult.TransactionNo, gatewayResponse);

                var order = payment.Order ?? await GetOrderByIdAsync(payment.OrderId);
                order.Confirm();

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "✅ Payment {PaymentId} synced as completed (TxnNo: {TransactionNo})",
                    payment.PaymentId, queryResult.TransactionNo);

                return true;
            }

            // Transaction failed
            _logger.LogWarning(
                "VNPay transaction failed - Payment: {PaymentId}, Status: {Status}",
                payment.PaymentId, queryResult.TransactionStatus);

            var failedResponse = JsonSerializer.Serialize(queryResult);
            payment.MarkAsFailed(failedResponse);

            var failedOrder = payment.Order ?? await GetOrderByIdAsync(payment.OrderId);
            failedOrder.Cancel();

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Payment {PaymentId} synced as failed",
                payment.PaymentId);

            return true;
        }

        private string GetTxnRefFromPayment(Payment payment)
        {
            // TODO: Implement proper TxnRef storage
            // Option 1: Add TxnRef column to Payment table (RECOMMENDED)
            // Option 2: Parse from GatewayResponse if available
            // Option 3: Use CreatedAt ticks (fallback)

            // For now, use CreatedAt ticks
            return $"{payment.OrderId}_{payment.CreatedAt.Ticks}";
        }

        private void ValidatePaymentCreation(int orderId, decimal amount, string paymentMethod)
        {
            if (orderId <= 0)
                throw new ArgumentException("OrderId must be positive", nameof(orderId));

            if (amount <= 0)
                throw new ArgumentException("Amount must be positive", nameof(amount));

            if (string.IsNullOrWhiteSpace(paymentMethod))
                throw new ArgumentException("PaymentMethod cannot be null or empty", nameof(paymentMethod));
        }

        private void ValidateTransactionId(string transactionId)
        {
            if (string.IsNullOrWhiteSpace(transactionId))
                throw new ArgumentException("TransactionId cannot be null or empty", nameof(transactionId));
        }

        // ============== HELPER CLASSES ==============

        private class BatchProcessResult
        {
            public int SuccessCount { get; set; }
            public List<string> Errors { get; set; } = new();
        }

        private class PaymentSettings
        {
            public int ExpirationMinutes { get; set; }
            public int BatchSize { get; set; }
            public int BatchDelayMs { get; set; }
        }
    }
}