using TwoWheelVehicleService.Models;

namespace TwoWheelVehicleService.Repositories
{
    public interface IModelRepository
    {
        Task<List<Model>> GetAllModels(); // Get All Models
        Task<List<Model>> GetActiveModels(); // Get Active Models
        Task AddModel(Model model);
        Task UpdateModel(Model model);
        Task DeleteModel(int modelId); // Soft delete
        Task<Model> GetModelById(int modelId);

    }
}
