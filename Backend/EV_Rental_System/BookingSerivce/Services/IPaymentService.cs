using BookingSerivce.Models;
using BookingService.DTOs;
using BookingService.Models;

namespace BookingSerivce.Services
{
    public interface IPaymentService
    {
        Task<Payment> CreatePaymentAsync(int orderId, string paymentMethod);
        Task<string?> CreateVNPayUrlAsync(int paymentId, bool isDeposit = false);
        Task<Payment> ProcessVNPayCallbackAsync(PaymentResponseModel vnpayResponse);
        Task<Payment?> GetPaymentByOrderIdAsync(int orderId);
        Task<Payment?> GetPaymentByTransactionCodeAsync(string transactionCode);
        Task UpdatePaymentStatusAsync(int paymentId, string status);
        Task<decimal> GetRemainingAmountAsync(int paymentId);
        Task<bool> CanProcessPaymentAsync(int paymentId, bool isDeposit);
    }

}
