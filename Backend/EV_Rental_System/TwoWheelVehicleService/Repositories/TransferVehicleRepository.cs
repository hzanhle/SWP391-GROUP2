using Microsoft.EntityFrameworkCore;
using TwoWheelVehicleService.Models;

namespace TwoWheelVehicleService.Repositories
{
    public class TransferVehicleRepository : ITransferVehicleRepository
    {
        private readonly MyDbContext _context;

        public TransferVehicleRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TransferVehicle>> GetTransferVehicles()
        {
            return await _context.TransferVehicles.OrderByDescending(tv => tv.CreateAt).ToListAsync();
        }

        public async Task<IEnumerable<TransferVehicle>> GetTransferVehiclesByModelId(int modelId)
        {
            return await _context.TransferVehicles
                .Where(tv => tv.ModelId == modelId)
                .ToListAsync();
        }

        public async Task<TransferVehicle?> GetTransferVehicleByVehicleId(int vehicleId)
        {
            return await _context.TransferVehicles
                .FirstOrDefaultAsync(tv => tv.VehicleId == vehicleId);
        }

        public async Task AddTransferVehicle(TransferVehicle transferVehicle)
        {
            _context.TransferVehicles.Add(transferVehicle);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTransferVehicle(TransferVehicle transferVehicle)
        {
            _context.TransferVehicles.Update(transferVehicle);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTransferVehicle(TransferVehicle transferVehicle)
        {
            _context.TransferVehicles.Remove(transferVehicle);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<TransferVehicle>> GetTransferVehiclesByStatus(string status)
        {
            return await _context.TransferVehicles.Where(tv => tv.TransferStatus.Equals(status)).ToListAsync();
        }

        
        
    }
}
