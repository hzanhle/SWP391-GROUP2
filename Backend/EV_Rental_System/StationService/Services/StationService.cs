using StationService.DTOs;
using StationService.Models;
using StationService.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

        public async Task<Station> AddStationAsync(CreateStationRequest stationRequest)
        {
            ValidateLatLng(stationRequest.Lat, stationRequest.Lng);
            // Chuyển từ Request DTO sang Model
            var newStation = new Station
            {
                Name = stationRequest.Name,
                Location = stationRequest.Location,
                ManagerId = stationRequest.ManagerId,
                IsActive = true, // Mặc định là active khi tạo mới
                Lat = stationRequest.Lat,
                Lng = stationRequest.Lng
            };
            var createdStation = await _stationRepository.AddStation(newStation);   
            return createdStation;
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

        public async Task<StationDTO?> GetStationByIdAsync(int stationId)
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
                IsActive = station.IsActive,

                Feedbacks = station.Feedbacks.Select(f => new FeedbackDTO
                {
                    FeedbackId =f.FeedbackId,
                    StationId = f.StationId,
                    Rate = f.Rate,
                    Description = f.Description,
                    CreatedDate = f.CreatedDate
                }).ToList()
            };
            return stationDTO;
        }

        public async Task UpdateStationAsync(int id, UpdateStationRequest stationRequest)
        {
            var existingStation = await _stationRepository.GetStationById(id);
            if (existingStation != null)
            {
                // Cập nhật thông tin từ object station được truyền vào
                existingStation.Name = stationRequest.Name;
                existingStation.Location = stationRequest.Location;
                existingStation.ManagerId = stationRequest.ManagerId;
                existingStation.IsActive = stationRequest.IsActive;

                await _stationRepository.UpdateStation(existingStation);
            }
            else
            {
                throw new KeyNotFoundException($"Không tìm thấy trạm với ID: {id} ");
            }
            await _stationRepository.UpdateStation(existingStation);
        }

        public async Task SetStatus(int stationId)
        {
            var station = await _stationRepository.GetStationById(stationId);
            if (station != null)
            {
                if (station.IsActive == true)
                {
                    station.IsActive = false;
                }
                else
                {
                    station.IsActive = true;
                }
                await _stationRepository.UpdateStation(station);
            }
        }

                // ---- Map helpers ----
        private static StationDTO ToDto(Station s) => new StationDTO(
s.Id, s.Name, s.Location, s.ManagerId, s.IsActive, s.Lat, s.Lng);

        private static void ValidateLatLng(double lat, double lng)
        {
            if (lat< -90 || lat> 90) throw new ArgumentException("Invalid latitude.");
            if (lng< -180 || lng> 180) throw new ArgumentException("Invalid longitude.");
        }

        public async Task<List<StationDTO>> GetStationsWithinBounds(double neLat, double neLng, double swLat, double swLng)
        {
            var list = await _stationRepository.GetWithinBounds(neLat, neLng, swLat, swLng);
            return list.Select(ToDto).ToList();
        }

        public async Task<List<StationDTO>> GetStationsNearby(double lat, double lng, double radiusKm)
        {
    ValidateLatLng(lat, lng);
    var list = await _stationRepository.GetNearby(lat, lng, radiusKm);
                return list.Select(ToDto).ToList();
            }

    }
}