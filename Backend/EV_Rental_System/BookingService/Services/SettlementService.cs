using BookingService.DTOs;
using BookingService.Models;
using BookingService.Models.ModelSettings;
using BookingService.Repositories;
using Microsoft.Extensions.Options;

namespace BookingService.Services
{
    public class SettlementService : ISettlementService
    {
        private readonly ISettlementRepository _settlementRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly ITrustScoreService _trustScoreService;
        private readonly BillingSettings _billingSettings;
        private readonly ILogger<SettlementService> _logger;

        public SettlementService(
            ISettlementRepository settlementRepo,
            IOrderRepository orderRepo,
            ITrustScoreService trustScoreService,
            IOptions<BillingSettings> billingSettings,
            ILogger<SettlementService> logger)
        {
            _settlementRepo = settlementRepo;
            _orderRepo = orderRepo;
            _trustScoreService = trustScoreService;
            _billingSettings = billingSettings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Calculate settlement preview (not saved to database)
        /// </summary>
        public async Task<SettlementResponse> CalculateSettlementAsync(int orderId, DateTime actualReturnTime)
        {
            _logger.LogInformation("Calculating settlement for Order {OrderId}", orderId);

            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order == null)
                throw new InvalidOperationException($"Order {orderId} not found");

            if (order.Status != OrderStatus.InProgress && order.Status != OrderStatus.Completed)
                throw new InvalidOperationException($"Cannot calculate settlement for Order {orderId} with status {order.Status}");

            // Create settlement entity for calculation
            var settlement = new Settlement
            {
                OrderId = orderId,
                Order = order,
                ScheduledReturnTime = order.ToDate,
                ActualReturnTime = actualReturnTime,
                InitialDeposit = order.DepositAmount
            };

            // Calculate overtime
            settlement.CalculateOvertime(
                hourlyRate: order.HourlyRate,
                overtimeMultiplier: _billingSettings.OvertimeRateMultiplier,
                gracePeriodMinutes: _billingSettings.OvertimeGracePeriodMinutes
            );

            // Calculate totals
            settlement.CalculateTotals();

            _logger.LogInformation(
                "Settlement calculated for Order {OrderId}: Overtime={OvertimeHours}h, OvertimeFee={OvertimeFee}, Refund={Refund}",
                orderId, settlement.OvertimeHours, settlement.OvertimeFee, settlement.DepositRefundAmount);

            return MapToResponse(settlement);
        }

        /// <summary>
        /// Create settlement and save to database
        /// </summary>
        public async Task<SettlementResponse> CreateSettlementAsync(int orderId, DateTime actualReturnTime)
        {
            _logger.LogInformation("Creating settlement for Order {OrderId}", orderId);

            // Check if settlement already exists
            if (await _settlementRepo.ExistsByOrderIdAsync(orderId))
                throw new InvalidOperationException($"Settlement already exists for Order {orderId}");

            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order == null)
                throw new InvalidOperationException($"Order {orderId} not found");

            if (order.Status != OrderStatus.InProgress && order.Status != OrderStatus.Completed)
                throw new InvalidOperationException($"Cannot create settlement for Order {orderId} with status {order.Status}");

            // Create settlement
            var settlement = new Settlement
            {
                OrderId = orderId,
                ScheduledReturnTime = order.ToDate,
                ActualReturnTime = actualReturnTime,
                InitialDeposit = order.DepositAmount
            };

            // Calculate overtime
            settlement.CalculateOvertime(
                hourlyRate: order.HourlyRate,
                overtimeMultiplier: _billingSettings.OvertimeRateMultiplier,
                gracePeriodMinutes: _billingSettings.OvertimeGracePeriodMinutes
            );

            // Calculate totals
            settlement.CalculateTotals();

            // Save to database
            var created = await _settlementRepo.CreateAsync(settlement);

            _logger.LogInformation("Settlement {SettlementId} created for Order {OrderId}", created.SettlementId, orderId);

            return MapToResponse(created);
        }

        /// <summary>
        /// Add damage charge to existing settlement
        /// </summary>
        public async Task<SettlementResponse> AddDamageChargeAsync(int orderId, decimal amount, string? description = null)
        {
            _logger.LogInformation("Adding damage charge {Amount} to Order {OrderId}", amount, orderId);

            var settlement = await _settlementRepo.GetByOrderIdAsync(orderId);
            if (settlement == null)
                throw new InvalidOperationException($"Settlement not found for Order {orderId}. Create settlement first.");

            if (settlement.IsFinalized)
                throw new InvalidOperationException($"Cannot add damage charge to finalized Settlement {settlement.SettlementId}");

            // Add damage
            settlement.AddDamageCharge(amount, description);

            // Recalculate totals
            settlement.CalculateTotals();

            // Save
            await _settlementRepo.UpdateAsync(settlement);

            _logger.LogInformation(
                "Damage charge added to Settlement {SettlementId}: Amount={Amount}, TotalDamage={TotalDamage}",
                settlement.SettlementId, amount, settlement.DamageCharge);

            return MapToResponse(settlement);
        }

        /// <summary>
        /// Finalize settlement (lock it in, trigger invoice generation)
        /// </summary>
        public async Task<SettlementResponse> FinalizeSettlementAsync(int orderId)
        {
            _logger.LogInformation("Finalizing settlement for Order {OrderId}", orderId);

            var settlement = await _settlementRepo.GetByOrderIdAsync(orderId);
            if (settlement == null)
                throw new InvalidOperationException($"Settlement not found for Order {orderId}");

            if (settlement.IsFinalized)
            {
                _logger.LogWarning("Settlement {SettlementId} is already finalized", settlement.SettlementId);
                return MapToResponse(settlement);
            }

            // Get order to retrieve userId for trust score updates
            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order == null)
                throw new InvalidOperationException($"Order {orderId} not found");

            // Finalize
            settlement.Complete();

            // Save
            await _settlementRepo.UpdateAsync(settlement);

            _logger.LogInformation("Settlement {SettlementId} finalized for Order {OrderId}", settlement.SettlementId, orderId);

            // ===== UPDATE TRUST SCORE BASED ON SETTLEMENT =====

            try
            {
                // Apply penalty for late return (if overtime > 0)
                if (settlement.OvertimeHours > 0)
                {
                    await _trustScoreService.UpdateScoreOnLateReturnAsync(
                        order.UserId,
                        orderId,
                        settlement.OvertimeHours);

                    _logger.LogInformation(
                        "Applied late return penalty for User {UserId}, Overtime: {OvertimeHours}h",
                        order.UserId, settlement.OvertimeHours);
                }

                // Apply penalty for damage (if damage charge > 0)
                if (settlement.DamageCharge > 0)
                {
                    await _trustScoreService.UpdateScoreOnDamageAsync(
                        order.UserId,
                        orderId,
                        settlement.DamageCharge);

                    _logger.LogInformation(
                        "Applied damage penalty for User {UserId}, Damage: {DamageCharge} VND",
                        order.UserId, settlement.DamageCharge);
                }

                // Note: Completion bonus (+10) is already applied in OrderService.CompleteRentalAsync()
                // Perfect return = +10 (from completion) - 0 (no penalties) = +10 net gain
                // Late/damage = +10 (from completion) - penalties = could be positive or negative
            }
            catch (Exception ex)
            {
                // Trust score update failure should not block settlement finalization
                _logger.LogError(ex, "Failed to update trust score for Order {OrderId}, but settlement was finalized", orderId);
            }

            // TODO: Trigger invoice generation here (will be implemented in SettlementInvoiceService)
            // if (_billingSettings.AutoGenerateInvoice)
            // {
            //     await _invoiceService.GenerateInvoiceAsync(settlement.SettlementId);
            // }

            return MapToResponse(settlement);
        }

        /// <summary>
        /// Get settlement by order ID
        /// </summary>
        public async Task<SettlementResponse?> GetSettlementByOrderIdAsync(int orderId)
        {
            var settlement = await _settlementRepo.GetByOrderIdAsync(orderId);
            return settlement == null ? null : MapToResponse(settlement);
        }

        /// <summary>
        /// Get settlement by settlement ID
        /// </summary>
        public async Task<SettlementResponse?> GetSettlementByIdAsync(int settlementId)
        {
            var settlement = await _settlementRepo.GetByIdAsync(settlementId);
            return settlement == null ? null : MapToResponse(settlement);
        }

        // ===== PRIVATE HELPERS =====

        /// <summary>
        /// Mark refund as processed (admin manually refunded via VNPay portal)
        /// </summary>
        public async Task<SettlementResponse> MarkRefundAsProcessedAsync(int orderId, int adminId, string? notes = null)
        {
            _logger.LogInformation("Marking refund as processed for Order {OrderId} by Admin {AdminId}", orderId, adminId);

            var settlement = await _settlementRepo.GetByOrderIdAsync(orderId);
            if (settlement == null)
                throw new InvalidOperationException($"Settlement not found for Order {orderId}");

            if (!settlement.IsFinalized)
                throw new InvalidOperationException($"Settlement must be finalized before processing refund");

            settlement.MarkRefundAsProcessed(adminId, notes);
            await _settlementRepo.UpdateAsync(settlement);

            _logger.LogInformation("Refund marked as processed for Order {OrderId}", orderId);

            return MapToResponse(settlement);
        }

        /// <summary>
        /// Mark refund as failed
        /// </summary>
        public async Task<SettlementResponse> MarkRefundAsFailedAsync(int orderId, int adminId, string? notes = null)
        {
            _logger.LogInformation("Marking refund as failed for Order {OrderId} by Admin {AdminId}", orderId, adminId);

            var settlement = await _settlementRepo.GetByOrderIdAsync(orderId);
            if (settlement == null)
                throw new InvalidOperationException($"Settlement not found for Order {orderId}");

            if (!settlement.IsFinalized)
                throw new InvalidOperationException($"Settlement must be finalized before processing refund");

            settlement.MarkRefundAsFailed(adminId, notes);
            await _settlementRepo.UpdateAsync(settlement);

            _logger.LogInformation("Refund marked as failed for Order {OrderId}", orderId);

            return MapToResponse(settlement);
        }

        private SettlementResponse MapToResponse(Settlement settlement)
        {
            return new SettlementResponse
            {
                SettlementId = settlement.SettlementId,
                OrderId = settlement.OrderId,
                ScheduledReturnTime = settlement.ScheduledReturnTime,
                ActualReturnTime = settlement.ActualReturnTime,
                OvertimeHours = settlement.OvertimeHours,
                OvertimeFee = settlement.OvertimeFee,
                DamageCharge = settlement.DamageCharge,
                DamageDescription = settlement.DamageDescription,
                InitialDeposit = settlement.InitialDeposit,
                TotalAdditionalCharges = settlement.TotalAdditionalCharges,
                DepositRefundAmount = settlement.DepositRefundAmount,
                AdditionalPaymentRequired = settlement.AdditionalPaymentRequired,
                IsFinalized = settlement.IsFinalized,
                InvoiceUrl = settlement.InvoiceUrl,
                CreatedAt = settlement.CreatedAt,
                FinalizedAt = settlement.FinalizedAt
            };
        }
    }
}
