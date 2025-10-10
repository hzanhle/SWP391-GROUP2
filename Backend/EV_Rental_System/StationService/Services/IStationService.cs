using StationService.DTOs;
using StationService.Models;
using StationService.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StationService.Services
{
    public interface IStationService
    {
        // Trả về danh sách Model
        Task<List<Station>> GetAllStationsAsync();

        // Trả về danh sách Model
        Task<List<Station>> GetActiveStationsAsync();

        // Nhận vào DTO Request
        Task AddStationAsync(CreateStationRequest stationRequest);

        // Nhận vào Model
        Task UpdateStationAsync(Station station);

        Task DeleteStationAsync(int stationId);

        // Trả về DTO
        Task<StationDTO> GetStationByIdAsync(int stationId);

        Task SetStatus(int stationId);
    }
    
}