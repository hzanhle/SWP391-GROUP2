using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using TwoWheelVehicleService.DTOs;
using TwoWheelVehicleService.Models;
using TwoWheelVehicleService.Services;

namespace TwoWheelVehicleService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 🔒 Yêu cầu xác thực cho tất cả endpoint
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;
        private readonly ILogger<VehicleService> _logger;

        public VehicleController(IVehicleService vehicleService, ILogger<VehicleService> logger)
        {
            _vehicleService = vehicleService;
            _logger = logger;
        }

        private IActionResult HandleInvalidModel()
        {
            var errors = ModelState
                .Where(x => x.Value.Errors.Any())
                .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage).ToArray() })
                .ToList();

            return BadRequest(new ResponseDTO
            {
                IsSuccess = false,
                Message = "Dữ liệu không hợp lệ",
                Data = errors
            });
        }

        // ============================ GET ============================

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllVehicles()
        {
            try
            {
                var vehicles = await _vehicleService.GetAllVehiclesAsync();
                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Lấy danh sách xe thành công",
                    Data = vehicles
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Lỗi hệ thống",
                    Data = ex.Message
                });
            }
        }

        [AllowAnonymous]
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveVehicles()
        {
            try
            {
                var vehicles = await _vehicleService.GetActiveVehiclesAsync();
                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Lấy danh sách xe active thành công",
                    Data = vehicles
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Lỗi hệ thống",
                    Data = ex.Message
                });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetVehicleById(int id)
        {
            try
            {
                var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
                if (vehicle == null)
                    return NotFound(new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Xe không tồn tại"
                    });

                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Lấy xe thành công",
                    Data = vehicle
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Lỗi hệ thống",
                    Data = ex.Message
                });
            }
        }

        [AllowAnonymous]
        [HttpGet("find-available")]
        public async Task<IActionResult> FindAvailableVehicle([FromQuery] VehicleBookingRequest request)
        {
            try
            {
                _logger.LogInformation("=== [FindAvailableVehicle] ===");
                _logger.LogInformation("ModelId: {ModelId}", request.ModelId);
                _logger.LogInformation("Color: {Color}", request.Color);
                _logger.LogInformation("StationId: {StationId}", request.StationId);

                var vehicle = await _vehicleService.GetAvailableVehicleForBooking(request);
                if (vehicle == null)
                {
                    return NotFound(new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Không tìm thấy xe phù hợp với yêu cầu"
                    });
                }

                _logger.LogInformation("Found vehicle: {LicensePlate}", vehicle.LicensePlate);

                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Tìm thấy xe khả dụng",
                    Data = vehicle
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding available vehicle");
                return StatusCode(500, new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Lỗi hệ thống nội bộ",
                    Data = ex.Message
                });
            }
        }

        // ============================ CREATE ============================

        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        public async Task<IActionResult> CreateVehicle([FromBody] VehicleRequest request)
        {
            if (!ModelState.IsValid)
                return HandleInvalidModel();

            try
            {
                await _vehicleService.AddVehicleAsync(request);
                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Tạo xe thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Lỗi hệ thống",
                    Data = ex.Message
                });
            }
        }

        // ============================ UPDATE ============================

        [Authorize(Roles = "Admin,Staff")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] Vehicle vehicle)
        {
            if (!ModelState.IsValid)
                return HandleInvalidModel();

            try
            {
                var existing = await _vehicleService.GetVehicleByIdAsync(id);
                if (existing == null)
                    return NotFound(new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Xe không tồn tại"
                    });

                vehicle.VehicleId = id;
                await _vehicleService.UpdateVehicleAsync(vehicle);

                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Cập nhật xe thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Lỗi hệ thống",
                    Data = ex.Message
                });
            }
        }

        // ============================ DELETE ============================

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            try
            {
                var existing = await _vehicleService.GetVehicleByIdAsync(id);
                if (existing == null)
                    return NotFound(new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Xe không tồn tại"
                    });

                await _vehicleService.DeleteVehicleAsync(id);
                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Xóa xe thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Lỗi hệ thống",
                    Data = ex.Message
                });
            }
        }

        // ============================ PATCH ============================

        [Authorize(Roles = "Admin,Staff")]
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateVehicleStatus(int id, [FromBody] string status)
        {
            if (string.IsNullOrEmpty(status))
                return BadRequest(new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Trạng thái không được để trống"
                });

            try
            {
                var existing = await _vehicleService.GetVehicleByIdAsync(id);
                if (existing == null)
                    return NotFound(new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Xe không tồn tại"
                    });

                await _vehicleService.SetVehicleStatus(id, status);
                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Cập nhật trạng thái thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Lỗi hệ thống",
                    Data = ex.Message
                });
            }
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpPatch("{id:int}/toggle")]
        public async Task<IActionResult> ToggleVehicleActiveStatus(int id)
        {
            try
            {
                await _vehicleService.ToggleActiveStatus(id);
                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Đổi trạng thái active xe thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Lỗi hệ thống",
                    Data = ex.Message
                });
            }
        }

        // ============================ CUSTOM API ============================

        [AllowAnonymous]
        [HttpPost("check-availability")]
        public async Task<IActionResult> CheckAvailability([FromBody] int vehicleId)
        {
            try
            {
                var vehicle = await _vehicleService.GetVehicleByIdAsync(vehicleId);
                if (vehicle == null)
                    return NotFound(new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Xe không tồn tại"
                    });

                bool isAvailable = (vehicle.IsActive ?? false) && vehicle.Status == "Available";

                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Kiểm tra khả dụng thành công",
                    Data = new
                    {
                        vehicle.VehicleId,
                        vehicle.ModelId,
                        vehicle.Color,
                        vehicle.Status,
                        vehicle.IsActive,
                        isAvailable
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Lỗi hệ thống",
                    Data = ex.Message
                });
            }
        }

        [AllowAnonymous]
        [HttpGet("available-by-model/{modelId:int}")]
        public async Task<IActionResult> GetAvailableVehiclesByModel(int modelId)
        {
            try
            {
                var allVehicles = await _vehicleService.GetAllVehiclesAsync();
                var available = allVehicles
                    .Where(v => v.ModelId == modelId && v.IsActive && v.Status == "Available")
                    .ToList();

                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = $"Có {available.Count} xe khả dụng cho model {modelId}",
                    Data = available
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Lỗi hệ thống",
                    Data = ex.Message
                });
            }
        }
    }
}
