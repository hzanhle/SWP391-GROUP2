using BookingService.DTOs;
using BookingService.Models;

namespace BookingService.Services
{
    public interface ISettlementService
    {
        /// <summary>
        /// Calculate settlement for an order (preview - not saved)
        /// </summary>
        Task<SettlementResponse> CalculateSettlementAsync(int orderId, DateTime actualReturnTime);

        /// <summary>
        /// Create and save a settlement for an order
        /// </summary>
        Task<SettlementResponse> CreateSettlementAsync(int orderId, DateTime actualReturnTime);

        /// <summary>
        /// Add damage charge to an existing settlement
        /// </summary>
        Task<SettlementResponse> AddDamageChargeAsync(int orderId, decimal amount, string? description = null);

        /// <summary>
        /// Finalize settlement and trigger invoice generation
        /// </summary>
        Task<SettlementResponse> FinalizeSettlementAsync(int orderId);

        /// <summary>
        /// Get settlement details by order ID
        /// </summary>
        Task<SettlementResponse?> GetSettlementByOrderIdAsync(int orderId);

        /// <summary>
        /// Get settlement details by settlement ID
        /// </summary>
        Task<SettlementResponse?> GetSettlementByIdAsync(int settlementId);

        /// <summary>
        /// Process automatic refund via VNPay Refund API
        /// </summary>
        Task<SettlementResponse> ProcessAutomaticRefundAsync(int orderId, int adminId);

        /// <summary>
        /// Mark refund as processed with proof document (manual refund via VNPay portal)
        /// </summary>
        Task<SettlementResponse> MarkRefundAsProcessedAsync(int orderId, int adminId, IFormFile proofDocument, string? notes = null);

        /// <summary>
        /// Mark refund as failed
        /// </summary>
        Task<SettlementResponse> MarkRefundAsFailedAsync(int orderId, int adminId, string? notes = null);

        /// <summary>
        /// Process additional payment callback from VNPay
        /// </summary>
        Task<SettlementResponse> ProcessAdditionalPaymentCallbackAsync(string txnRef, IQueryCollection queryParams);

        /// <summary>
        /// Get or regenerate additional payment URL for a settlement
        /// </summary>
        Task<string> GetAdditionalPaymentUrlAsync(int orderId);

        /// <summary>
        /// Check additional payment status for a settlement
        /// </summary>
        Task<AdditionalPaymentStatus> CheckAdditionalPaymentStatusAsync(int orderId);
    }
}
