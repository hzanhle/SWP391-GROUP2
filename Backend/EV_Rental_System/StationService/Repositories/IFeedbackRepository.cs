using StationService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StationService.Repositories
{
    public interface IFeedbackRepository
    {
        Task<Feedback?> GetByIdAsync(int feedbackId);
        Task<List<Feedback>> GetAllAsync();
        Task<Feedback> CreateAsync(Feedback feedback);
        Task<Feedback> UpdateAsync(Feedback feedback);
        Task<bool> DeleteAsync(int feedbackId);

        // Query methods
        Task<List<Feedback>> GetByStationIdAsync(int stationId, bool onlyPublished = true);
        Task<List<Feedback>> GetByUserIdAsync(int userId);
        Task<Feedback?> GetByUserAndStationAsync(int userId, int stationId);

        // Stats
        Task<int> GetTotalFeedbacksForStationAsync(int stationId);
        Task<double> GetAverageRatingForStationAsync(int stationId);
    }
}
