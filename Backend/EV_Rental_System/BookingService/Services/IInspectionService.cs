using BookingService.DTOs.Inspection;
using BookingService.Models.Enums;

namespace BookingService.Services
{
    public interface IInspectionService
    {
        // === INSPECTION OPERATIONS ===
        Task<InspectionResponse> CreateInspectionAsync(CreateInspectionRequest request);
        Task<InspectionDetailDto?> GetInspectionDetailsAsync(int inspectionId);
        Task<List<InspectionResponse>> GetInspectionsByOrderIdAsync(int orderId);
        Task<InspectionDetailDto?> GetInspectionByOrderAndTypeAsync(int orderId, InspectionType type);
        Task<bool> DeleteInspectionAsync(int inspectionId);

        // === DAMAGE OPERATIONS ===
        Task<DamageDto> AddDamageToInspectionAsync(int inspectionId, AddDamageRequest request);
        Task<bool> UpdateDamageAsync(int damageId, AddDamageRequest request);
        Task<bool> DeleteDamageAsync(int damageId);

        // === PHOTO OPERATIONS ===
        Task<string> UploadInspectionPhotoAsync(int inspectionId, IFormFile photo, string photoType);
        Task<List<PhotoDto>> GetInspectionPhotosAsync(int inspectionId);
        Task<bool> DeletePhotoAsync(int photoId);

        // === VALIDATION ===
        Task<bool> CanCreatePickupInspectionAsync(int orderId);
        Task<bool> CanCreateReturnInspectionAsync(int orderId);
    }
}
