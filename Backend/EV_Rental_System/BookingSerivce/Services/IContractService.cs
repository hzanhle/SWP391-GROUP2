using BookingSerivce.DTOs;
using BookingService.Models;

namespace BookingSerivce.Services
{
    public interface IContractService
    {
        Task<OnlineContract> GenerateContractAsync(int orderId, int templateVersion = 1);
        Task<OnlineContract?> GetContractByIdAsync(int contractId);
        Task<OnlineContract?> GetContractByOrderIdAsync(int orderId);
        Task<OnlineContract> SignContractAsync(int contractId, string signatureData, string ipAddress);
        Task<string> GetContractTermsAsync(int orderId);

        // Stage 2 Enhancement - Auto PDF generation after payment
        Task<OnlineContract> GenerateContractWithPdfAsync(int orderId);
        Task<byte[]> GetContractPdfAsync(int contractId);
    }
}
