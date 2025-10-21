﻿using BookingService.Models;
using BookingService.DTOs; // Cần ContractDetailsDto
using System.Threading.Tasks;

namespace BookingService.Services
{
    public interface IOnlineContractService
    {
        Task<string> GeneratePdfAsync(ContractDataDto contractData);
        // New method
        Task<ContractDetailsDto> CreateContractFromDataAsync(ContractDataDto contractData);
    }
}