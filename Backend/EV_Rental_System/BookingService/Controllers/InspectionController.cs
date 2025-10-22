using BookingService.DTOs;
using BookingService.DTOs.Inspection;
using BookingService.Models.Enums;
using BookingService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.Controllers
{
    [ApiController]
    [Route("api/inspections")]
    [Authorize] // Require authentication for all endpoints
    public class InspectionController : ControllerBase
    {
        private readonly IInspectionService _inspectionService;
        private readonly ILogger<InspectionController> _logger;

        public InspectionController(
            IInspectionService inspectionService,
            ILogger<InspectionController> logger)
        {
            _inspectionService = inspectionService;
            _logger = logger;
        }

        /// <summary>
        /// Create a pickup inspection (when customer picks up vehicle)
        /// Employee or Member can create
        /// </summary>
        [HttpPost("pickup/{orderId}")]
        [Authorize(Roles = "Employee,Member")]
        public async Task<IActionResult> CreatePickupInspection(
            int orderId,
            [FromBody] CreateInspectionRequest request)
        {
            try
            {
                // Validate can create pickup inspection
                var canCreate = await _inspectionService.CanCreatePickupInspectionAsync(orderId);
                if (!canCreate)
                {
                    return BadRequest(new ResponseDTO
                    {
                        Message = "Cannot create pickup inspection. Order must be Confirmed and not have existing pickup inspection."
                    });
                }

                request.OrderId = orderId;
                request.InspectionType = InspectionType.Pickup;

                var result = await _inspectionService.CreateInspectionAsync(request);

                return Ok(new
                {
                    success = true,
                    message = "Pickup inspection created successfully",
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation creating pickup inspection for Order {OrderId}", orderId);
                return BadRequest(new ResponseDTO { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating pickup inspection for Order {OrderId}", orderId);
                return StatusCode(500, new ResponseDTO { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Create a return inspection (when customer returns vehicle)
        /// Employee or Member can create
        /// </summary>
        [HttpPost("return/{orderId}")]
        [Authorize(Roles = "Employee,Member")]
        public async Task<IActionResult> CreateReturnInspection(
            int orderId,
            [FromBody] CreateInspectionRequest request)
        {
            try
            {
                // Validate can create return inspection
                var canCreate = await _inspectionService.CanCreateReturnInspectionAsync(orderId);
                if (!canCreate)
                {
                    return BadRequest(new ResponseDTO
                    {
                        Message = "Cannot create return inspection. Order must be InProgress, have pickup inspection, and not have existing return inspection."
                    });
                }

                request.OrderId = orderId;
                request.InspectionType = InspectionType.Return;

                var result = await _inspectionService.CreateInspectionAsync(request);

                return Ok(new
                {
                    success = true,
                    message = "Return inspection created successfully",
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation creating return inspection for Order {OrderId}", orderId);
                return BadRequest(new ResponseDTO { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating return inspection for Order {OrderId}", orderId);
                return StatusCode(500, new ResponseDTO { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get inspection details by ID
        /// </summary>
        [HttpGet("{inspectionId}")]
        [Authorize(Roles = "Admin,Employee,Member")]
        public async Task<IActionResult> GetInspectionById(int inspectionId)
        {
            try
            {
                var inspection = await _inspectionService.GetInspectionDetailsAsync(inspectionId);
                if (inspection == null)
                {
                    return NotFound(new ResponseDTO
                    {
                        Message = $"Inspection {inspectionId} not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = inspection
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inspection {InspectionId}", inspectionId);
                return StatusCode(500, new ResponseDTO { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get all inspections for an order
        /// </summary>
        [HttpGet("order/{orderId}")]
        [Authorize(Roles = "Admin,Employee,Member")]
        public async Task<IActionResult> GetInspectionsByOrderId(int orderId)
        {
            try
            {
                var inspections = await _inspectionService.GetInspectionsByOrderIdAsync(orderId);

                return Ok(new
                {
                    success = true,
                    count = inspections.Count,
                    data = inspections
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inspections for Order {OrderId}", orderId);
                return StatusCode(500, new ResponseDTO { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get pickup or return inspection for an order
        /// </summary>
        [HttpGet("order/{orderId}/{inspectionType}")]
        [Authorize(Roles = "Admin,Employee,Member")]
        public async Task<IActionResult> GetInspectionByOrderAndType(
            int orderId,
            string inspectionType)
        {
            try
            {
                if (!Enum.TryParse<InspectionType>(inspectionType, true, out var type))
                {
                    return BadRequest(new ResponseDTO
                    {
                        Message = "Invalid inspection type. Use 'Pickup' or 'Return'"
                    });
                }

                var inspection = await _inspectionService.GetInspectionByOrderAndTypeAsync(orderId, type);
                if (inspection == null)
                {
                    return NotFound(new ResponseDTO
                    {
                        Message = $"{inspectionType} inspection not found for Order {orderId}"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = inspection
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting {InspectionType} inspection for Order {OrderId}",
                    inspectionType, orderId);
                return StatusCode(500, new ResponseDTO { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Add damage record to an inspection
        /// </summary>
        [HttpPost("{inspectionId}/damages")]
        [Authorize(Roles = "Employee,Member")]
        public async Task<IActionResult> AddDamage(
            int inspectionId,
            [FromBody] AddDamageRequest request)
        {
            try
            {
                var damage = await _inspectionService.AddDamageToInspectionAsync(inspectionId, request);

                return Ok(new
                {
                    success = true,
                    message = "Damage added successfully",
                    data = damage
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation adding damage to inspection {InspectionId}", inspectionId);
                return BadRequest(new ResponseDTO { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding damage to inspection {InspectionId}", inspectionId);
                return StatusCode(500, new ResponseDTO { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update damage record
        /// </summary>
        [HttpPut("damages/{damageId}")]
        [Authorize(Roles = "Employee,Admin")]
        public async Task<IActionResult> UpdateDamage(
            int damageId,
            [FromBody] AddDamageRequest request)
        {
            try
            {
                var success = await _inspectionService.UpdateDamageAsync(damageId, request);
                if (!success)
                {
                    return NotFound(new ResponseDTO
                    {
                        Message = $"Damage {damageId} not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Damage updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating damage {DamageId}", damageId);
                return StatusCode(500, new ResponseDTO { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete damage record
        /// </summary>
        [HttpDelete("damages/{damageId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDamage(int damageId)
        {
            try
            {
                var success = await _inspectionService.DeleteDamageAsync(damageId);
                if (!success)
                {
                    return NotFound(new ResponseDTO
                    {
                        Message = $"Damage {damageId} not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Damage deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting damage {DamageId}", damageId);
                return StatusCode(500, new ResponseDTO { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Upload photo for inspection
        /// </summary>
        [HttpPost("{inspectionId}/photos")]
        [Authorize(Roles = "Employee,Member")]
        public async Task<IActionResult> UploadPhoto(
            int inspectionId,
            [FromForm] IFormFile photo,
            [FromForm] string photoType)
        {
            try
            {
                if (photo == null || photo.Length == 0)
                {
                    return BadRequest(new ResponseDTO
                    {
                        Message = "Photo file is required"
                    });
                }

                var photoUrl = await _inspectionService.UploadInspectionPhotoAsync(
                    inspectionId,
                    photo,
                    photoType);

                return Ok(new
                {
                    success = true,
                    message = "Photo uploaded successfully",
                    photoUrl
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation uploading photo for inspection {InspectionId}", inspectionId);
                return BadRequest(new ResponseDTO { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument uploading photo");
                return BadRequest(new ResponseDTO { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading photo for inspection {InspectionId}", inspectionId);
                return StatusCode(500, new ResponseDTO { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get all photos for an inspection
        /// </summary>
        [HttpGet("{inspectionId}/photos")]
        [Authorize(Roles = "Admin,Employee,Member")]
        public async Task<IActionResult> GetInspectionPhotos(int inspectionId)
        {
            try
            {
                var photos = await _inspectionService.GetInspectionPhotosAsync(inspectionId);

                return Ok(new
                {
                    success = true,
                    count = photos.Count,
                    data = photos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting photos for inspection {InspectionId}", inspectionId);
                return StatusCode(500, new ResponseDTO { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete inspection (admin only, used for corrections)
        /// </summary>
        [HttpDelete("{inspectionId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteInspection(int inspectionId)
        {
            try
            {
                var success = await _inspectionService.DeleteInspectionAsync(inspectionId);
                if (!success)
                {
                    return NotFound(new ResponseDTO
                    {
                        Message = $"Inspection {inspectionId} not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Inspection deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting inspection {InspectionId}", inspectionId);
                return StatusCode(500, new ResponseDTO { Message = "Internal server error" });
            }
        }
    }
}

/*
 * ===== INSPECTION CONTROLLER - API ENDPOINTS =====
 *
 * CREATE INSPECTIONS:
 * POST /api/inspections/pickup/{orderId}     - Create pickup inspection
 * POST /api/inspections/return/{orderId}     - Create return inspection
 *
 * GET INSPECTIONS:
 * GET /api/inspections/{inspectionId}                    - Get inspection details
 * GET /api/inspections/order/{orderId}                   - Get all inspections for order
 * GET /api/inspections/order/{orderId}/{inspectionType}  - Get specific inspection type
 *
 * DAMAGE MANAGEMENT:
 * POST   /api/inspections/{inspectionId}/damages  - Add damage to inspection
 * PUT    /api/inspections/damages/{damageId}      - Update damage
 * DELETE /api/inspections/damages/{damageId}      - Delete damage
 *
 * PHOTO MANAGEMENT:
 * POST /api/inspections/{inspectionId}/photos  - Upload photo
 * GET  /api/inspections/{inspectionId}/photos  - Get all photos
 *
 * ADMIN OPERATIONS:
 * DELETE /api/inspections/{inspectionId}  - Delete inspection (corrections only)
 *
 * ===== AUTHORIZATION =====
 * Employee, Member: Create inspections, add damages, upload photos
 * Admin, Employee, Member: View inspections
 * Admin, Employee: Update damages
 * Admin: Delete inspections and damages
 */
