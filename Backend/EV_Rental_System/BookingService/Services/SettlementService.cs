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
        private readonly IPaymentRepository _paymentRepo;
        private readonly ITrustScoreService _trustScoreService;
        private readonly IVNPayService _vnpayService;
        private readonly IAwsS3Service _s3Service;
        private readonly BillingSettings _billingSettings;
        private readonly ILogger<SettlementService> _logger;

        public SettlementService(
            ISettlementRepository settlementRepo,
            IOrderRepository orderRepo,
            IPaymentRepository paymentRepo,
            ITrustScoreService trustScoreService,
            IVNPayService vnpayService,
            IAwsS3Service s3Service,
            IOptions<BillingSettings> billingSettings,
            ILogger<SettlementService> logger)
        {
            _settlementRepo = settlementRepo;
            _orderRepo = orderRepo;
            _paymentRepo = paymentRepo;
            _trustScoreService = trustScoreService;
            _vnpayService = vnpayService;
            _s3Service = s3Service;
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

            // Link to original deposit payment
            var depositPayment = order.GetDepositPayment();
            if (depositPayment != null)
            {
                settlement.OriginalPaymentId = depositPayment.PaymentId;
            }

            // Handle additional payment if deposit is insufficient
            if (settlement.AdditionalPaymentRequired > 0)
            {
                // Check if amount is too small (< 100 VND) - treat as negligible due to rounding errors
                if (settlement.AdditionalPaymentRequired < 100)
                {
                    _logger.LogInformation(
                        "Additional payment amount {Amount} VND for Order {OrderId} is negligible (< 100 VND). Treating as fully covered.",
                        settlement.AdditionalPaymentRequired, orderId);

                    settlement.AdditionalPaymentRequired = 0;
                    settlement.AdditionalPaymentStatus = AdditionalPaymentStatus.NotRequired;

                    // Save settlement without creating payment
                    var created = await _settlementRepo.CreateAsync(settlement);
                    _logger.LogInformation("Settlement {SettlementId} created for Order {OrderId} (no additional payment needed)",
                        created.SettlementId, orderId);
                    return MapToResponse(created);
                }

                _logger.LogInformation("Deposit insufficient for Order {OrderId}. Additional payment required: {Amount}",
                    orderId, settlement.AdditionalPaymentRequired);

                settlement.AdditionalPaymentStatus = AdditionalPaymentStatus.Pending;

                // Save settlement first to get SettlementId
                var tempSettlement = await _settlementRepo.CreateAsync(settlement);

                // Create payment record for additional charge
                var additionalPayment = new Payment
                {
                    OrderId = orderId,
                    Type = PaymentType.AdditionalCharge,
                    SettlementId = tempSettlement.SettlementId,
                    Amount = settlement.AdditionalPaymentRequired,
                    PaymentMethod = "VNPay",
                    Status = PaymentStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(24) // 24 hour payment window
                };

                var createdPaymentId = await _paymentRepo.CreateAsync(additionalPayment);

                // Generate VNPay payment URL
                var paymentUrl = _vnpayService.CreateAdditionalPaymentUrl(
                    tempSettlement.SettlementId,
                    orderId,
                    settlement.AdditionalPaymentRequired,
                    $"Additional charges for Order #{orderId}"
                );

                // Update settlement with payment info
                tempSettlement.AdditionalPaymentId = createdPaymentId;
                tempSettlement.AdditionalPaymentUrl = paymentUrl;
                await _settlementRepo.UpdateAsync(tempSettlement);

                _logger.LogInformation("Additional payment {PaymentId} created for Settlement {SettlementId}",
                    createdPaymentId, tempSettlement.SettlementId);

                return MapToResponse(tempSettlement);
            }
            else
            {
                // No additional payment required
                settlement.AdditionalPaymentStatus = AdditionalPaymentStatus.NotRequired;

                // Save to database
                var created = await _settlementRepo.CreateAsync(settlement);

                _logger.LogInformation("Settlement {SettlementId} created for Order {OrderId}", created.SettlementId, orderId);

                return MapToResponse(created);
            }
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

            // Check if additional payment is required but not completed
            if (settlement.AdditionalPaymentStatus == AdditionalPaymentStatus.Pending)
            {
                throw new InvalidOperationException(
                    $"Cannot finalize Settlement {settlement.SettlementId}. " +
                    $"Additional payment of {settlement.AdditionalPaymentRequired} VND is required but not yet paid. " +
                    $"Payment URL: {settlement.AdditionalPaymentUrl}");
            }

            if (settlement.AdditionalPaymentStatus == AdditionalPaymentStatus.Failed)
            {
                throw new InvalidOperationException(
                    $"Cannot finalize Settlement {settlement.SettlementId}. " +
                    $"Additional payment failed. Please retry payment.");
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
        /// Process automatic refund via VNPay Refund API
        /// </summary>
        public async Task<SettlementResponse> ProcessAutomaticRefundAsync(int orderId, int adminId)
        {
            _logger.LogInformation("Processing automatic refund for Order {OrderId} by Admin {AdminId}", orderId, adminId);

            var settlement = await _settlementRepo.GetByOrderIdAsync(orderId);
            if (settlement == null)
                throw new InvalidOperationException($"Settlement not found for Order {orderId}");

            if (!settlement.IsFinalized)
                throw new InvalidOperationException($"Settlement must be finalized before processing refund");

            if (settlement.RefundStatus == RefundStatus.Processed)
                throw new InvalidOperationException($"Refund already processed for Order {orderId}");

            // Check if refund is required
            if (settlement.DepositRefundAmount <= 0)
            {
                settlement.RefundStatus = RefundStatus.NotRequired;
                settlement.RefundMethod = RefundMethod.NotSet;
                settlement.RefundNotes = "No refund required - customer owes money or deposit fully consumed";
                await _settlementRepo.UpdateAsync(settlement);
                return MapToResponse(settlement);
            }

            // Get original payment
            var payment = await _paymentRepo.GetByOrderIdAsync(orderId);
            if (payment == null)
                throw new InvalidOperationException($"Payment not found for Order {orderId}");

            if (payment.Status != PaymentStatus.Completed)
                throw new InvalidOperationException($"Cannot refund payment that is not completed (Status: {payment.Status})");

            if (string.IsNullOrEmpty(payment.TransactionId))
                throw new InvalidOperationException($"Payment {payment.PaymentId} has no transaction ID");

            // Update settlement status to Processing
            settlement.RefundStatus = RefundStatus.Processing;
            settlement.RefundMethod = RefundMethod.Automatic;
            settlement.OriginalPaymentId = payment.PaymentId;
            await _settlementRepo.UpdateAsync(settlement);

            // Call VNPay Refund API
            var refundResponse = await _vnpayService.ProcessRefundAsync(
                txnRef: payment.TransactionId,
                amount: settlement.DepositRefundAmount,
                orderInfo: $"Refund for Order {orderId}",
                transactionNo: payment.TransactionId.Split('_')[0], // Extract transaction number
                transactionDate: payment.CreatedAt,
                createdBy: adminId
            );

            if (refundResponse == null)
            {
                // VNPay API call failed
                settlement.RefundStatus = RefundStatus.AwaitingManualProof;
                settlement.RefundNotes = "Automatic refund failed - VNPay API returned null. Please process refund manually and upload proof document.";
                await _settlementRepo.UpdateAsync(settlement);

                _logger.LogWarning("Automatic refund failed for Order {OrderId} - VNPay API returned null", orderId);
                return MapToResponse(settlement);
            }

            // Store VNPay response
            settlement.RefundTransactionId = refundResponse.TransactionNo;
            settlement.RefundGatewayResponse = refundResponse.RawResponse;

            if (refundResponse.IsSuccess)
            {
                // Refund successful
                settlement.RefundStatus = RefundStatus.Processed;
                settlement.RefundProcessedAt = DateTime.UtcNow;
                settlement.RefundProcessedBy = adminId;
                settlement.RefundNotes = $"Automatic refund successful via VNPay API: {refundResponse.Message}";

                _logger.LogInformation("Automatic refund successful for Order {OrderId}, TransactionNo: {TransNo}",
                    orderId, refundResponse.TransactionNo);
            }
            else
            {
                // Refund failed - fallback to manual
                settlement.RefundStatus = RefundStatus.AwaitingManualProof;
                settlement.RefundNotes = $"Automatic refund failed: {refundResponse.Message} (Code: {refundResponse.ResponseCode}). Please process refund manually and upload proof document.";

                _logger.LogWarning("Automatic refund failed for Order {OrderId}: {Message}",
                    orderId, refundResponse.Message);
            }

            await _settlementRepo.UpdateAsync(settlement);
            return MapToResponse(settlement);
        }

        /// <summary>
        /// Mark refund as processed with proof document (manual refund via VNPay portal)
        /// </summary>
        public async Task<SettlementResponse> MarkRefundAsProcessedAsync(int orderId, int adminId, IFormFile proofDocument, string? notes = null)
        {
            _logger.LogInformation("Marking refund as processed for Order {OrderId} by Admin {AdminId} with proof document", orderId, adminId);

            var settlement = await _settlementRepo.GetByOrderIdAsync(orderId);
            if (settlement == null)
                throw new InvalidOperationException($"Settlement not found for Order {orderId}");

            if (!settlement.IsFinalized)
                throw new InvalidOperationException($"Settlement must be finalized before processing refund");

            if (settlement.RefundStatus == RefundStatus.Processed)
                throw new InvalidOperationException($"Refund already processed for Order {orderId}");

            // Validate proof document
            if (proofDocument == null || proofDocument.Length == 0)
                throw new ArgumentException("Proof document is required for manual refund");

            // Validate file type (image or PDF)
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
            var extension = Path.GetExtension(proofDocument.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException($"Invalid file type. Allowed: {string.Join(", ", allowedExtensions)}");

            // Validate file size (max 5MB)
            if (proofDocument.Length > 5 * 1024 * 1024)
                throw new ArgumentException("File size cannot exceed 5MB");

            // Upload proof document to S3
            string? proofUrl;
            using (var stream = proofDocument.OpenReadStream())
            {
                var fileName = $"refund-proof/order-{orderId}-{DateTime.UtcNow.Ticks}{extension}";
                proofUrl = await _s3Service.UploadFileAsync(stream, fileName, proofDocument.ContentType);

                if (string.IsNullOrEmpty(proofUrl))
                    throw new InvalidOperationException("Failed to upload proof document to S3");
            }

            // Check if refund is required
            if (settlement.DepositRefundAmount <= 0)
            {
                settlement.RefundStatus = RefundStatus.NotRequired;
            }
            else
            {
                settlement.RefundStatus = RefundStatus.Processed;
            }

            settlement.RefundMethod = RefundMethod.Manual;
            settlement.RefundProcessedAt = DateTime.UtcNow;
            settlement.RefundProcessedBy = adminId;
            settlement.RefundNotes = notes;
            settlement.RefundProofDocumentUrl = proofUrl;
            settlement.RefundProofUploadedAt = DateTime.UtcNow;

            await _settlementRepo.UpdateAsync(settlement);

            _logger.LogInformation("Refund marked as processed for Order {OrderId} with proof document at {ProofUrl}", orderId, proofUrl);

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

        /// <summary>
        /// Process additional payment callback from VNPay
        /// </summary>
        public async Task<SettlementResponse> ProcessAdditionalPaymentCallbackAsync(string txnRef, IQueryCollection queryParams)
        {
            _logger.LogInformation("Processing additional payment callback for TxnRef: {TxnRef}", txnRef);

            // Validate VNPay signature
            if (!_vnpayService.ValidateCallback(queryParams))
            {
                _logger.LogWarning("Invalid VNPay signature for TxnRef: {TxnRef}", txnRef);
                throw new InvalidOperationException("Invalid payment callback signature");
            }

            // Extract settlementId from txnRef (format: SETTLEMENT_{settlementId}_{orderId}_{tick})
            var parts = txnRef.Split('_');
            if (parts.Length < 4 || parts[0] != "SETTLEMENT")
            {
                throw new InvalidOperationException($"Invalid TxnRef format: {txnRef}");
            }

            var settlementId = int.Parse(parts[1]);
            var settlement = await _settlementRepo.GetByIdAsync(settlementId);
            if (settlement == null)
                throw new InvalidOperationException($"Settlement {settlementId} not found");

            // Get payment record
            var payment = settlement.AdditionalPayment;
            if (payment == null)
                throw new InvalidOperationException($"Additional payment record not found for Settlement {settlementId}");

            // Check payment status from VNPay response
            var responseCode = queryParams["vnp_ResponseCode"].ToString();
            var transactionNo = queryParams["vnp_TransactionNo"].ToString();
            var amountStr = queryParams["vnp_Amount"].ToString();
            var amount = decimal.Parse(amountStr) / 100; // VNPay amount is in cents

            if (responseCode == "00")
            {
                // Payment successful
                payment.MarkAsCompleted(transactionNo, queryParams.ToString());
                settlement.AdditionalPaymentStatus = AdditionalPaymentStatus.Completed;

                _logger.LogInformation(
                    "Additional payment completed for Settlement {SettlementId}. Transaction: {TransactionNo}, Amount: {Amount}",
                    settlementId, transactionNo, amount);
            }
            else
            {
                // Payment failed
                payment.MarkAsFailed(queryParams.ToString());
                settlement.AdditionalPaymentStatus = AdditionalPaymentStatus.Failed;

                _logger.LogWarning(
                    "Additional payment failed for Settlement {SettlementId}. ResponseCode: {ResponseCode}",
                    settlementId, responseCode);
            }

            await _paymentRepo.UpdateAsync(payment);
            await _settlementRepo.UpdateAsync(settlement);

            return MapToResponse(settlement);
        }

        /// <summary>
        /// Get or regenerate additional payment URL for a settlement
        /// </summary>
        public async Task<string> GetAdditionalPaymentUrlAsync(int orderId)
        {
            _logger.LogInformation("Getting additional payment URL for Order {OrderId}", orderId);

            // Use tracked query since we'll be updating the settlement
            var settlement = await _settlementRepo.GetByOrderIdForUpdateAsync(orderId);
            if (settlement == null)
                throw new InvalidOperationException($"Settlement not found for Order {orderId}");

            if (settlement.AdditionalPaymentStatus != AdditionalPaymentStatus.Pending)
            {
                throw new InvalidOperationException(
                    $"Additional payment is not pending for Order {orderId}. Status: {settlement.AdditionalPaymentStatus}");
            }

            // If URL already exists and payment is still pending, return it
            if (!string.IsNullOrEmpty(settlement.AdditionalPaymentUrl))
            {
                var payment = settlement.AdditionalPayment;
                if (payment != null && payment.Status == PaymentStatus.Pending && !payment.IsExpired)
                {
                    _logger.LogInformation("Returning existing payment URL for Order {OrderId}", orderId);
                    return settlement.AdditionalPaymentUrl;
                }
            }

            // Generate new payment URL
            var newPaymentUrl = _vnpayService.CreateAdditionalPaymentUrl(
                settlement.SettlementId,
                orderId,
                settlement.AdditionalPaymentRequired,
                $"Additional charges for Order #{orderId} (Regenerated)"
            );

            settlement.AdditionalPaymentUrl = newPaymentUrl;
            await _settlementRepo.UpdateAsync(settlement);

            _logger.LogInformation("Generated new payment URL for Order {OrderId}", orderId);
            return newPaymentUrl;
        }

        /// <summary>
        /// Check additional payment status for a settlement
        /// </summary>
        public async Task<AdditionalPaymentStatus> CheckAdditionalPaymentStatusAsync(int orderId)
        {
            _logger.LogInformation("Checking additional payment status for Order {OrderId}", orderId);

            var settlement = await _settlementRepo.GetByOrderIdAsync(orderId);
            if (settlement == null)
                throw new InvalidOperationException($"Settlement not found for Order {orderId}");

            return settlement.AdditionalPaymentStatus;
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
                FinalizedAt = settlement.FinalizedAt,
                RefundStatus = settlement.RefundStatus.ToString(),
                RefundMethod = settlement.RefundMethod.ToString(),
                RefundProcessedAt = settlement.RefundProcessedAt,
                RefundProcessedBy = settlement.RefundProcessedBy,
                RefundNotes = settlement.RefundNotes,
                RefundTransactionId = settlement.RefundTransactionId,
                RefundProofDocumentUrl = settlement.RefundProofDocumentUrl,
                RefundProofUploadedAt = settlement.RefundProofUploadedAt,
                OriginalPaymentId = settlement.OriginalPaymentId,
                AdditionalPaymentStatus = settlement.AdditionalPaymentStatus.ToString(),
                AdditionalPaymentId = settlement.AdditionalPaymentId,
                AdditionalPaymentUrl = settlement.AdditionalPaymentUrl
            };
        }
    }
}
