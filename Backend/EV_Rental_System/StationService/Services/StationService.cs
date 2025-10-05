// File: Services/StationService.cs
using StationService.DTOs;
using StationService.Models;
using StationService.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StationService.Services
{
    public class StationService : IStationService
    {
        private readonly IStationRepository _stationRepository;

        public StationService(IStationRepository stationRepository)
        {
            _stationRepository = stationRepository;
        }

        public async Task AddStationAsync(CreateStationRequest stationRequest)
        {
            // Chuyển từ Request DTO sang Model
            var newStation = new Station
            {
                Name = stationRequest.Name,
                Location = stationRequest.Location,
                ManagerId = stationRequest.ManagerId,
                IsActive = true // Mặc định là active khi tạo mới
            };

            await _stationRepository.AddStation(newStation);
        }

        public async Task DeleteStationAsync(int stationId)
        {
            await _stationRepository.DeleteStation(stationId);
        }

        public async Task<List<Station>> GetActiveStationsAsync()
        {
            // Lấy trực tiếp từ Repository và trả về Model
            return await _stationRepository.GetActiveStations();
        }

        public async Task<List<Station>> GetAllStationsAsync()
        {
            // Lấy trực tiếp từ Repository và trả về Model
            return await _stationRepository.GetAllStations();
        }

        public async Task<StationDTO> GetStationByIdAsync(int stationId)
        {
            var station = await _stationRepository.GetStationById(stationId);
            if (station == null)
            {
                return null;
            }

            // Chuyển từ Model sang DTO để trả về
            var stationDTO = new StationDTO
            {
                Id = station.Id,
                Name = station.Name,
                Location = station.Location,
                ManagerId = station.ManagerId,
                IsActive = station.IsActive
            };
            return stationDTO;
        }

        public async Task UpdateStationAsync(Station station)
        {
            var existingStation = await _stationRepository.GetStationById(station.Id);
            if (existingStation != null)
            {
                // Cập nhật thông tin từ object station được truyền vào
                existingStation.Name = station.Name;
                existingStation.Location = station.Location;
                existingStation.ManagerId = station.ManagerId;
                existingStation.IsActive = station.IsActive;

                await _stationRepository.UpdateStation(existingStation);

            }
        }
    }
}