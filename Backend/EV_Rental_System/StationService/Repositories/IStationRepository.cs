using StationService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StationService.Repositories
{
    public interface IStationRepository
    {
        Task<Station> AddStation(Station station);
        Task UpdateStation(Station station);
        Task<Station?> GetStationById(int id);
        Task<List<Station>> GetAllStations();
        Task DeleteStation(int id);
        Task<List<Station>> GetActiveStations();
        Task<List<Station>> GetInactiveStations();
        Task<List<Station>> GetWithinBounds(double neLat, double neLng, double swLat, double swLng);
        Task<List<Station>> GetNearby(double lat, double lng, double radiusKm);

    }
}