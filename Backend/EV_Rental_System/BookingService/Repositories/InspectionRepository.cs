using BookingService.Models;
using BookingService.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Repositories
{
    public class InspectionRepository : IInspectionRepository
    {
        private readonly MyDbContext _context;

        public InspectionRepository(MyDbContext context)
        {
            _context = context;
        }

        // === INSPECTION CRUD ===

        public async Task<VehicleInspection> CreateInspectionAsync(VehicleInspection inspection)
        {
            _context.VehicleInspections.Add(inspection);
            await _context.SaveChangesAsync();
            return inspection;
        }

        public async Task<VehicleInspection?> GetInspectionByIdAsync(int inspectionId)
        {
            return await _context.VehicleInspections
                .FirstOrDefaultAsync(i => i.InspectionId == inspectionId);
        }

        public async Task<VehicleInspection?> GetInspectionWithDetailsAsync(int inspectionId)
        {
            return await _context.VehicleInspections
                .Include(i => i.Damages)
                .Include(i => i.Photos)
                .FirstOrDefaultAsync(i => i.InspectionId == inspectionId);
        }

        public async Task<List<VehicleInspection>> GetInspectionsByOrderIdAsync(int orderId)
        {
            return await _context.VehicleInspections
                .Where(i => i.OrderId == orderId)
                .OrderBy(i => i.InspectionDate)
                .ToListAsync();
        }

        public async Task<VehicleInspection?> GetInspectionByOrderAndTypeAsync(int orderId, InspectionType type)
        {
            return await _context.VehicleInspections
                .Include(i => i.Damages)
                .Include(i => i.Photos)
                .FirstOrDefaultAsync(i => i.OrderId == orderId && i.InspectionType == type);
        }

        public async Task<List<VehicleInspection>> GetInspectionsByVehicleIdAsync(int vehicleId)
        {
            return await _context.VehicleInspections
                .Where(i => i.VehicleId == vehicleId)
                .OrderByDescending(i => i.InspectionDate)
                .ToListAsync();
        }

        public async Task<bool> UpdateInspectionAsync(VehicleInspection inspection)
        {
            _context.VehicleInspections.Update(inspection);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteInspectionAsync(int inspectionId)
        {
            var inspection = await GetInspectionByIdAsync(inspectionId);
            if (inspection == null) return false;

            _context.VehicleInspections.Remove(inspection);
            return await _context.SaveChangesAsync() > 0;
        }

        // === DAMAGE CRUD ===

        public async Task<InspectionDamage> AddDamageAsync(InspectionDamage damage)
        {
            _context.InspectionDamages.Add(damage);
            await _context.SaveChangesAsync();
            return damage;
        }

        public async Task<InspectionDamage?> GetDamageByIdAsync(int damageId)
        {
            return await _context.InspectionDamages
                .FirstOrDefaultAsync(d => d.DamageId == damageId);
        }

        public async Task<List<InspectionDamage>> GetDamagesByInspectionIdAsync(int inspectionId)
        {
            return await _context.InspectionDamages
                .Where(d => d.InspectionId == inspectionId)
                .OrderBy(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> UpdateDamageAsync(InspectionDamage damage)
        {
            _context.InspectionDamages.Update(damage);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteDamageAsync(int damageId)
        {
            var damage = await GetDamageByIdAsync(damageId);
            if (damage == null) return false;

            _context.InspectionDamages.Remove(damage);
            return await _context.SaveChangesAsync() > 0;
        }

        // === PHOTO CRUD ===

        public async Task<InspectionPhoto> AddPhotoAsync(InspectionPhoto photo)
        {
            _context.InspectionPhotos.Add(photo);
            await _context.SaveChangesAsync();
            return photo;
        }

        public async Task<List<InspectionPhoto>> GetPhotosByInspectionIdAsync(int inspectionId)
        {
            return await _context.InspectionPhotos
                .Where(p => p.InspectionId == inspectionId)
                .OrderBy(p => p.UploadedAt)
                .ToListAsync();
        }

        public async Task<bool> DeletePhotoAsync(int photoId)
        {
            var photo = await _context.InspectionPhotos
                .FirstOrDefaultAsync(p => p.PhotoId == photoId);
            if (photo == null) return false;

            _context.InspectionPhotos.Remove(photo);
            return await _context.SaveChangesAsync() > 0;
        }

        // === BUSINESS QUERIES ===

        public async Task<bool> HasPickupInspectionAsync(int orderId)
        {
            return await _context.VehicleInspections
                .AnyAsync(i => i.OrderId == orderId && i.InspectionType == InspectionType.Pickup);
        }

        public async Task<bool> HasReturnInspectionAsync(int orderId)
        {
            return await _context.VehicleInspections
                .AnyAsync(i => i.OrderId == orderId && i.InspectionType == InspectionType.Return);
        }

        public async Task<decimal> GetTotalDamageCostAsync(int inspectionId)
        {
            var damages = await GetDamagesByInspectionIdAsync(inspectionId);
            return damages.Sum(d => d.EstimatedCost ?? 0);
        }
    }
}
