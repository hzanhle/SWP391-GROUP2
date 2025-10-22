using BookingService.Models;
using BookingService.DTOs; // Cần ContractDetailsDto
using System.Threading.Tasks;

namespace BookingService.Services
{
    public interface IOnlineContractService
    {
        Task<string> GeneratePdfAsync(ContractDataDto contractData);
        // New method
        Task<ContractDetailsDto> CreateContractFromDataAsync(ContractDataDto contractData);

        // Contract Download & Retrieval
        Task<(byte[] FileBytes, string FileName, string ContentType)> GetContractFileAsync(string fileName, int userId, string userRole);
        Task<ContractDetailsDto> GetContractByOrderIdAsync(int orderId, int userId, string userRole);
    }
}