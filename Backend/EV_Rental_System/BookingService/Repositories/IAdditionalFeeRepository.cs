using BookingService.Models;

namespace BookingService.Repositories
{
    public interface IAdditionalFeeRepository
    {
        Task<AdditionalFee?> GetByIdAsync(int feeId);
        Task<List<AdditionalFee>> GetByOrderIdAsync(int orderId);
        Task<AdditionalFee> AddAsync(AdditionalFee fee);
        Task<bool> UpdateAsync(AdditionalFee fee);
        Task<bool> DeleteAsync(int feeId);
        Task<List<AdditionalFee>> GetUnpaidFeesByOrderIdAsync(int orderId);
        Task<decimal> GetTotalFeesByOrderIdAsync(int orderId);
        Task<bool> MarkAsPaidAsync(int feeId);
    }
}
