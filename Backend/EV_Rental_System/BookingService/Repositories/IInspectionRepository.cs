using BookingService.Models;
using BookingService.Models.Enums;

namespace BookingService.Repositories
{
    public interface IInspectionRepository
    {
        // === INSPECTION CRUD ===
        Task<VehicleInspection> CreateInspectionAsync(VehicleInspection inspection);
        Task<VehicleInspection?> GetInspectionByIdAsync(int inspectionId);
        Task<VehicleInspection?> GetInspectionWithDetailsAsync(int inspectionId);
        Task<List<VehicleInspection>> GetInspectionsByOrderIdAsync(int orderId);
        Task<VehicleInspection?> GetInspectionByOrderAndTypeAsync(int orderId, InspectionType type);
        Task<List<VehicleInspection>> GetInspectionsByVehicleIdAsync(int vehicleId);
        Task<bool> UpdateInspectionAsync(VehicleInspection inspection);
        Task<bool> DeleteInspectionAsync(int inspectionId);

        // === DAMAGE CRUD ===
        Task<InspectionDamage> AddDamageAsync(InspectionDamage damage);
        Task<InspectionDamage?> GetDamageByIdAsync(int damageId);
        Task<List<InspectionDamage>> GetDamagesByInspectionIdAsync(int inspectionId);
        Task<bool> UpdateDamageAsync(InspectionDamage damage);
        Task<bool> DeleteDamageAsync(int damageId);

        // === PHOTO CRUD ===
        Task<InspectionPhoto> AddPhotoAsync(InspectionPhoto photo);
        Task<List<InspectionPhoto>> GetPhotosByInspectionIdAsync(int inspectionId);
        Task<bool> DeletePhotoAsync(int photoId);

        // === BUSINESS QUERIES ===
        Task<bool> HasPickupInspectionAsync(int orderId);
        Task<bool> HasReturnInspectionAsync(int orderId);
        Task<decimal> GetTotalDamageCostAsync(int inspectionId);
    }
}
