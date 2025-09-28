using TwoWheelVehicleService.DTOs;

namespace TwoWheelVehicleService.Services
{
    public interface IModelService
    {
        Task AddModelAsync(ModelRequest request);
        Task<ModelDTO> GetModelByIdAsync(int modelId);
        Task<List<ModelDTO>> GetAllModelsAsync();
        Task<List<ModelDTO>> GetActiveModelsAsync();
        Task UpdateModelAsync(int modelId, ModelRequest request);
        Task DeleteModelAsync(int modelId);
    }
}