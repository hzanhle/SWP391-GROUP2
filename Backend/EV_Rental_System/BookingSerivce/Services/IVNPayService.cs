using BookingSerivce.Models;
using BookingService.DTOs;
using Microsoft.Extensions.Options;
namespace BookingSerivce.Services
{
    public interface IVNPayService
    {
        Task<string> CreatePaymentUrl(PaymentInformationModel model, string ipAddress);
        Task<PaymentResponseModel> ProcessPaymentResponse(Dictionary<string, string> queryParams);
    }
}