using BookingService.DTOs.Inspection;
using BookingService.Models;
using BookingService.Models.Enums;
using BookingService.Repositories;

namespace BookingService.Services
{
    public class InspectionService : IInspectionService
    {
        private readonly IInspectionRepository _inspectionRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<InspectionService> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly string _photoStoragePath;

        public InspectionService(
            IInspectionRepository inspectionRepository,
            IOrderRepository orderRepository,
            ILogger<InspectionService> logger,
            IWebHostEnvironment environment)
        {
            _inspectionRepository = inspectionRepository;
            _orderRepository = orderRepository;
            _logger = logger;
            _environment = environment;
            _photoStoragePath = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "inspections");

            // Ensure directory exists
            if (!Directory.Exists(_photoStoragePath))
            {
                Directory.CreateDirectory(_photoStoragePath);
            }
        }

        // === INSPECTION OPERATIONS ===

        public async Task<InspectionResponse> CreateInspectionAsync(CreateInspectionRequest request)
        {
            try
            {
                // Validate order exists
                var order = await _orderRepository.GetByIdAsync(request.OrderId);
                if (order == null)
                {
                    throw new InvalidOperationException($"Order {request.OrderId} not found");
                }

                // Check if inspection already exists for this type
                var existing = await _inspectionRepository.GetInspectionByOrderAndTypeAsync(
                    request.OrderId,
                    request.InspectionType);

                if (existing != null)
                {
                    throw new InvalidOperationException(
                        $"{request.InspectionType} inspection already exists for Order {request.OrderId}");
                }

                // Create inspection
                var inspection = new VehicleInspection
                {
                    OrderId = request.OrderId,
                    VehicleId = request.VehicleId,
                    InspectionType = request.InspectionType,
                    InspectorUserId = request.InspectorUserId,
                    InspectionDate = DateTime.UtcNow,
                    Mileage = request.Mileage,
                    BatteryLevel = request.BatteryLevel,
                    OverallCondition = request.OverallCondition,
                    Notes = request.Notes
                };

                var created = await _inspectionRepository.CreateInspectionAsync(inspection);

                _logger.LogInformation(
                    "Created {InspectionType} inspection {InspectionId} for Order {OrderId}",
                    request.InspectionType, created.InspectionId, request.OrderId);

                return MapToResponse(created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating inspection for Order {OrderId}", request.OrderId);
                throw;
            }
        }

        public async Task<InspectionDetailDto?> GetInspectionDetailsAsync(int inspectionId)
        {
            var inspection = await _inspectionRepository.GetInspectionWithDetailsAsync(inspectionId);
            if (inspection == null) return null;

            return MapToDetailDto(inspection);
        }

        public async Task<List<InspectionResponse>> GetInspectionsByOrderIdAsync(int orderId)
        {
            var inspections = await _inspectionRepository.GetInspectionsByOrderIdAsync(orderId);
            return inspections.Select(MapToResponse).ToList();
        }

        public async Task<InspectionDetailDto?> GetInspectionByOrderAndTypeAsync(int orderId, InspectionType type)
        {
            var inspection = await _inspectionRepository.GetInspectionByOrderAndTypeAsync(orderId, type);
            if (inspection == null) return null;

            return MapToDetailDto(inspection);
        }

        public async Task<bool> DeleteInspectionAsync(int inspectionId)
        {
            return await _inspectionRepository.DeleteInspectionAsync(inspectionId);
        }

        // === DAMAGE OPERATIONS ===

        public async Task<DamageDto> AddDamageToInspectionAsync(int inspectionId, AddDamageRequest request)
        {
            try
            {
                var inspection = await _inspectionRepository.GetInspectionByIdAsync(inspectionId);
                if (inspection == null)
                {
                    throw new InvalidOperationException($"Inspection {inspectionId} not found");
                }

                var damage = new InspectionDamage
                {
                    InspectionId = inspectionId,
                    DamageType = request.DamageType,
                    Location = request.Location,
                    Severity = request.Severity,
                    Description = request.Description,
                    EstimatedCost = request.EstimatedCost,
                    PhotoUrl = request.PhotoUrl
                };

                var created = await _inspectionRepository.AddDamageAsync(damage);

                _logger.LogInformation(
                    "Added damage {DamageId} to inspection {InspectionId}",
                    created.DamageId, inspectionId);

                return MapToDamageDto(created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding damage to inspection {InspectionId}", inspectionId);
                throw;
            }
        }

        public async Task<bool> UpdateDamageAsync(int damageId, AddDamageRequest request)
        {
            var damage = await _inspectionRepository.GetDamageByIdAsync(damageId);
            if (damage == null) return false;

            damage.DamageType = request.DamageType;
            damage.Location = request.Location;
            damage.Severity = request.Severity;
            damage.Description = request.Description;
            damage.EstimatedCost = request.EstimatedCost;
            damage.PhotoUrl = request.PhotoUrl;

            return await _inspectionRepository.UpdateDamageAsync(damage);
        }

        public async Task<bool> DeleteDamageAsync(int damageId)
        {
            return await _inspectionRepository.DeleteDamageAsync(damageId);
        }

        // === PHOTO OPERATIONS ===

        public async Task<string> UploadInspectionPhotoAsync(int inspectionId, IFormFile photo, string photoType)
        {
            try
            {
                // Validate inspection exists
                var inspection = await _inspectionRepository.GetInspectionByIdAsync(inspectionId);
                if (inspection == null)
                {
                    throw new InvalidOperationException($"Inspection {inspectionId} not found");
                }

                // Validate file
                if (photo == null || photo.Length == 0)
                {
                    throw new ArgumentException("Photo file is required");
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    throw new ArgumentException("Only JPG, JPEG, and PNG files are allowed");
                }

                // Max 5MB
                if (photo.Length > 5 * 1024 * 1024)
                {
                    throw new ArgumentException("Photo size must be less than 5MB");
                }

                // Create unique filename
                var fileName = $"{inspectionId}_{photoType}_{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(_photoStoragePath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }

                // Save record to database
                var photoUrl = $"/uploads/inspections/{fileName}";
                var photoRecord = new InspectionPhoto
                {
                    InspectionId = inspectionId,
                    PhotoUrl = photoUrl,
                    PhotoType = photoType
                };

                await _inspectionRepository.AddPhotoAsync(photoRecord);

                _logger.LogInformation(
                    "Uploaded photo for inspection {InspectionId}: {PhotoUrl}",
                    inspectionId, photoUrl);

                return photoUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading photo for inspection {InspectionId}", inspectionId);
                throw;
            }
        }

        public async Task<List<PhotoDto>> GetInspectionPhotosAsync(int inspectionId)
        {
            var photos = await _inspectionRepository.GetPhotosByInspectionIdAsync(inspectionId);
            return photos.Select(p => new PhotoDto
            {
                PhotoId = p.PhotoId,
                PhotoUrl = p.PhotoUrl,
                PhotoType = p.PhotoType,
                UploadedAt = p.UploadedAt
            }).ToList();
        }

        public async Task<bool> DeletePhotoAsync(int photoId)
        {
            // TODO: Also delete physical file
            return await _inspectionRepository.DeletePhotoAsync(photoId);
        }

        // === VALIDATION ===

        public async Task<bool> CanCreatePickupInspectionAsync(int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) return false;

            // Can create pickup inspection if order is Confirmed
            if (order.Status != OrderStatus.Confirmed)
                return false;

            // Check if pickup inspection already exists
            return !await _inspectionRepository.HasPickupInspectionAsync(orderId);
        }

        public async Task<bool> CanCreateReturnInspectionAsync(int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) return false;

            // Can create return inspection if order is InProgress
            if (order.Status != OrderStatus.InProgress)
                return false;

            // Must have pickup inspection first
            if (!await _inspectionRepository.HasPickupInspectionAsync(orderId))
                return false;

            // Check if return inspection already exists
            return !await _inspectionRepository.HasReturnInspectionAsync(orderId);
        }

        // === HELPER METHODS ===

        private InspectionResponse MapToResponse(VehicleInspection inspection)
        {
            return new InspectionResponse
            {
                InspectionId = inspection.InspectionId,
                OrderId = inspection.OrderId,
                VehicleId = inspection.VehicleId,
                InspectionType = inspection.InspectionType.ToString(),
                InspectorUserId = inspection.InspectorUserId,
                InspectionDate = inspection.InspectionDate,
                Mileage = inspection.Mileage,
                BatteryLevel = inspection.BatteryLevel,
                OverallCondition = inspection.OverallCondition?.ToString(),
                Notes = inspection.Notes,
                CreatedAt = inspection.CreatedAt,
                DamageCount = inspection.Damages?.Count ?? 0,
                PhotoCount = inspection.Photos?.Count ?? 0
            };
        }

        private InspectionDetailDto MapToDetailDto(VehicleInspection inspection)
        {
            return new InspectionDetailDto
            {
                InspectionId = inspection.InspectionId,
                OrderId = inspection.OrderId,
                VehicleId = inspection.VehicleId,
                InspectionType = inspection.InspectionType.ToString(),
                InspectorUserId = inspection.InspectorUserId,
                InspectionDate = inspection.InspectionDate,
                Mileage = inspection.Mileage,
                BatteryLevel = inspection.BatteryLevel,
                OverallCondition = inspection.OverallCondition?.ToString(),
                Notes = inspection.Notes,
                CreatedAt = inspection.CreatedAt,
                Damages = inspection.Damages?.Select(MapToDamageDto).ToList() ?? new List<DamageDto>(),
                Photos = inspection.Photos?.Select(p => new PhotoDto
                {
                    PhotoId = p.PhotoId,
                    PhotoUrl = p.PhotoUrl,
                    PhotoType = p.PhotoType,
                    UploadedAt = p.UploadedAt
                }).ToList() ?? new List<PhotoDto>()
            };
        }

        private DamageDto MapToDamageDto(InspectionDamage damage)
        {
            return new DamageDto
            {
                DamageId = damage.DamageId,
                InspectionId = damage.InspectionId,
                DamageType = damage.DamageType,
                Location = damage.Location,
                Severity = damage.Severity.ToString(),
                Description = damage.Description,
                EstimatedCost = damage.EstimatedCost,
                PhotoUrl = damage.PhotoUrl,
                CreatedAt = damage.CreatedAt
            };
        }
    }
}
