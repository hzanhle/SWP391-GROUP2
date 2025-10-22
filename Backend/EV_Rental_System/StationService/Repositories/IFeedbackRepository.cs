using StationService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StationService.Repositories
{
    public interface IFeedbackRepository
    {
        Task<Feedback?> GetByIdAsync(int id);
        Task<IEnumerable<Feedback>> GetByStationIdAsync(int stationId);
        Task<Feedback> AddAsync(Feedback feedback);
        Task UpdateAsync(Feedback feedback);
        Task DeleteAsync(int id);
    }
}
