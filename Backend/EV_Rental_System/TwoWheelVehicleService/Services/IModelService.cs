using TwoWheelVehicleService.DTOs;
using TwoWheelVehicleService.Models;

namespace TwoWheelVehicleService.Services
{
    public interface IModelService
    {
        Task<List<ModelDTO>> GetAllModelsAsync();
        Task<List<ModelDTO>> GetActiveModelsAsync();
        Task AddModelAsync(ModelRequest request);
        Task UpdateModelAsync(int modelId, ModelRequest request);
        Task DeleteModelAsync(int modelId);
        Task<ModelDTO> GetModelByIdAsync(int modelId);
        
        
    }
}
