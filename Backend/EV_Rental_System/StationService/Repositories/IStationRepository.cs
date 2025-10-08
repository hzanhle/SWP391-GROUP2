using StationService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StationService.Repositories
{
    public interface IStationRepository
    {
        Task AddStation(Station station);
        Task UpdateStation(Station station);
        Task<Station> GetStationById(int id);
        Task<List<Station>> GetAllStations();
        Task DeleteStation(int id);
        Task<List<Station>> GetActiveStations();
        Task<List<Station>> GetInactiveStations();
        Task<List<Station>> GetStationsByManagerId(int managerId);
    }
}