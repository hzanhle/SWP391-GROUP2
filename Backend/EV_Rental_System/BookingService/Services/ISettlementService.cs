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
    }
}
