using Microsoft.EntityFrameworkCore;
using StationService.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StationService.Repositories
{
    public class StationRepository : IStationRepository
    {
        private readonly MyDbContext _context;

        public StationRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<List<Station>> GetAllStations()
        {
            return await _context.Stations.ToListAsync();
        }

        public async Task<List<Station>> GetActiveStations()
        {
            return await _context.Stations.Where(s => s.IsActive).ToListAsync();
        }

        public async Task<List<Station>> GetInactiveStations()
        {
            return await _context.Stations.Where(s => !s.IsActive).ToListAsync();
        }

        public async Task<List<Station>> GetStationsByManagerId(int managerId)
        {
            return await _context.Stations
                                 .Where(s => s.ManagerId == managerId)
                                 .ToListAsync();
        }

        
        public async Task AddStation(Station station)
        {
            await _context.Stations.AddAsync(station);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteStation(int id)
        {
            var station = await _context.Stations.FindAsync(id);
            if (station != null)
            {
                _context.Stations.Remove(station);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Station> GetStationById(int id)
        {
            return await _context.Stations.FindAsync(id);
        }

        public async Task UpdateStation(Station station)
        {
            _context.Entry(station).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}