using TwoWheelVehicleService.Models;

namespace TwoWheelVehicleService.Repositories
{
    public interface IModelRepository
    {
        Task AddModel(Model model);
        Task<Model> GetModelById(int modelId);
        Task<List<Model>> GetAllModels();
        Task<List<Model>> GetActiveModels();
        Task UpdateModel(Model model);
        Task DeleteModel(int modelId);
    }
}