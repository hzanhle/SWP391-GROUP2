using Microsoft.EntityFrameworkCore;
using StationService.Models;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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


        public async Task<Station> AddStation(Station station)
        {
            await _context.Stations.AddAsync(station);
            await _context.SaveChangesAsync();
            return station;
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

        public async Task<Station?> GetStationById(int id)
        {
            return await _context.Stations
                         .Include(s => s.Feedbacks)
                         .Include(s => s.StaffShifts)
                         .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task UpdateStation(Station station)
        {
            _context.Entry(station).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

                // ---- Map queries ----
        public async Task<List<Station>> GetWithinBounds(double neLat, double neLng, double swLat, double swLng)
        {
            // Không wrap kinh độ
            if (neLng >= swLng)
                return await _context.Stations.AsNoTracking()
                    .Where(s => s.Lat <= neLat && s.Lat >= swLat && s.Lng <= neLng && s.Lng >= swLng)
                    .ToListAsync();

            // Trường hợp map wrap qua kinh tuyến 180°
            return await _context.Stations.AsNoTracking()
                .Where(s => s.Lat <= neLat && s.Lat >= swLat && (s.Lng <= neLng || s.Lng >= swLng))
                .ToListAsync();
        }

        public async Task<List<Station>> GetNearby(double lat, double lng, double radiusKm)
        {
            const double KM_PER_DEG_LAT = 110.574;
            double kmPerDegLng = 111.320 * Math.Cos(lat * Math.PI / 180.0);
            double dLat = radiusKm / KM_PER_DEG_LAT;
            double dLng = radiusKm / kmPerDegLng;

            double minLat = lat - dLat, maxLat = lat + dLat;
            double minLng = lng - dLng, maxLng = lng + dLng;

            var pre = await _context.Stations.AsNoTracking()
                .Where(s => s.Lat >= minLat && s.Lat <= maxLat && s.Lng >= minLng && s.Lng <= maxLng)
                .ToListAsync();

            static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
                        const double R = 6371;
        double dLat = (lat2 - lat1) * Math.PI / 180.0;
        double dLon = (lon2 - lon1) * Math.PI / 180.0;
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
        Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
        Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                        return R * c;
                    }
            return pre.Where(s => Haversine(lat, lng, s.Lat, s.Lng) <= radiusKm).ToList();
       }
    }
}