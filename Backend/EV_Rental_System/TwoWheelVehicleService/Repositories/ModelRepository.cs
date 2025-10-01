using Microsoft.EntityFrameworkCore;
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

        public async Task ChangeStatus(int modelId)
        {
            var model = await _context.Models.FindAsync(modelId);
            if (model != null && model.IsActive == true)
            {
                model.IsActive = false; // Soft delete by setting IsActive to false
                await _context.SaveChangesAsync();
            } else
            {
                model.IsActive = true;
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

        public async Task UpdateModel(Model model)
        {
            _context.Models.Update(model);
            await _context.SaveChangesAsync();
            }
        } 
    }

