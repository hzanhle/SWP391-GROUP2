using Microsoft.EntityFrameworkCore;
using TwoWheelVehicleService.DTOs;
using TwoWheelVehicleService.Models;

namespace TwoWheelVehicleService.Repositories
{
    public class ModelRepository : IModelRepository
    {
        private readonly MyDbContext _context;

        public ModelRepository(MyDbContext context)
        {
            _context = context;
        }
        public async Task AddModel(Model model)
        {
            await _context.Models.AddAsync(model);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteModel(int modelId)
        {
            var model = await _context.Models.FindAsync(modelId);
            if (model != null)
            {
                _context.Models.Remove(model);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Model>> GetActiveModels()
        {
            return await _context.Models.Where(m => m.IsActive).ToListAsync();
        }

        public async Task<List<Model>> GetAllModels()
        {
            return await _context.Models.ToListAsync();
        }

        public Task<Model> GetModelById(int modelId)
        {
            return _context.Models.FirstOrDefaultAsync(m => m.ModelId == modelId);
        }

        public Task<List<Model>?> SearchModelsAsync(string searchValue)
        {
            return _context.Models
                .Where(m => m.ModelName.Contains(searchValue) || m.Manufacturer.Contains(searchValue)).ToListAsync();
        }

        public async Task UpdateModel(Model model)
        {
            _context.Models.Update(model);
            await _context.SaveChangesAsync();
            }
        } 
    }

