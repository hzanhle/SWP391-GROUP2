using BookingService.DTOs.Fees;
using BookingService.Models;

namespace BookingService.Services
{
    public interface IFeeCalculationService
    {
        /// <summary>
        /// Automatically calculate all applicable fees for an order
        /// </summary>
        Task<FeeCalculationResponse> CalculateFeesAsync(FeeCalculationRequest request);

        /// <summary>
        /// Manually add a fee to an order
        /// </summary>
        Task<FeeDto> AddFeeAsync(AddFeeRequest request);

        /// <summary>
        /// Get all fees for a specific order
        /// </summary>
        Task<List<FeeDto>> GetFeesByOrderIdAsync(int orderId);

        /// <summary>
        /// Get a specific fee by ID
        /// </summary>
        Task<FeeDto?> GetFeeByIdAsync(int feeId);

        /// <summary>
        /// Mark a fee as paid
        /// </summary>
        Task<bool> MarkFeeAsPaidAsync(int feeId);

        /// <summary>
        /// Delete a fee (only unpaid fees)
        /// </summary>
        Task<bool> DeleteFeeAsync(int feeId);

        /// <summary>
        /// Get total fees amount for an order
        /// </summary>
        Task<decimal> GetTotalFeesAsync(int orderId);
    }
}
