using StationService.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StationService.Services
{
    public interface IFeedbackService
    {
        Task<FeedbackDTO?> GetByIdAsync(int id);
        Task<IEnumerable<FeedbackDTO>> GetByStationIdAsync(int stationId);
        Task<FeedbackDTO> CreateAsync(int stationId, CreateFeedbackRequest request);
        Task UpdateAsync(int id, UpdateFeedbackRequest request);
        Task DeleteAsync(int id);
    }
}
