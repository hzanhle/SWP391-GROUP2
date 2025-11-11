using BookingSerivce.DTOs;

namespace BookingService.Services
{
    public interface IVNPayService
    {
        string CreatePaymentUrl(int orderId, decimal amount, string description);
        string CreateAdditionalPaymentUrl(int settlementId, int orderId, decimal amount, string description);
        bool ValidateCallback(IQueryCollection query);
        Task<VNPayQueryResponse?> QueryTransactionAsync(string txnRef, DateTime transactionDate);
        Task<VNPayRefundResponse?> ProcessRefundAsync(string txnRef, decimal amount, string orderInfo, string transactionNo, DateTime transactionDate, int createdBy);
    }
}