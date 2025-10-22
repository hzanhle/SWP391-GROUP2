using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public VehicleController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        // ============================ GET ============================

        [HttpGet]
        [AllowAnonymous] // 🟡 Có thể cho phép public xem danh sách xe
        public async Task<IActionResult> GetAllVehicles()
        {
            try
            {
                var vehicles = await _vehicleService.GetAllVehiclesAsync();
                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO { Message = ex.Message });
            }
        }

        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveVehicles()
        {
            try
            {
                var vehicles = await _vehicleService.GetActiveVehiclesAsync();
                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO { Message = ex.Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetVehicleById(int id)
        {
            try
            {
                var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
                if (vehicle == null)
                    return NotFound(new ResponseDTO { Message = "Vehicle not found" });

                return Ok(vehicle);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO { Message = ex.Message });
            }
        }

        // ============================ CREATE ============================

        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        public async Task<IActionResult> CreateVehicle([FromBody] VehicleRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Any())
                        .Select(x => new
                        {
                            Field = x.Key,
                            Errors = x.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        })
                        .ToList();

                    return BadRequest(new ResponseDTO
                    {
                        Message = "Invalid data",
                        Data = errors
                    });
                }

                await _vehicleService.AddVehicleAsync(request);
                return Ok(new ResponseDTO { Message = "Vehicle created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO { Message = ex.Message });
            }
        }

        // ============================ UPDATE ============================

        [Authorize(Roles = "Admin,Staff")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] Vehicle vehicle)
        {
            try
            {
                var existing = await _vehicleService.GetVehicleByIdAsync(id);
                if (existing == null)
                    return NotFound(new ResponseDTO { Message = "Vehicle not found" });

                vehicle.VehicleId = id;
                await _vehicleService.UpdateVehicleAsync(vehicle);

                return Ok(new ResponseDTO { Message = "Vehicle updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO { Message = ex.Message });
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
                    return NotFound(new ResponseDTO { Message = "Vehicle not found" });

                await _vehicleService.DeleteVehicleAsync(id);
                return Ok(new ResponseDTO { Message = "Vehicle deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO { Message = ex.Message });
            }
        }

        // ============================ PATCH ============================

        [Authorize(Roles = "Admin,Staff")]
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateVehicleStatus(int id, [FromBody] string status)
        {
            try
            {
                if (string.IsNullOrEmpty(status))
                    return BadRequest(new ResponseDTO { Message = "Status is required" });

                var existing = await _vehicleService.GetVehicleByIdAsync(id);
                if (existing == null)
                    return NotFound(new ResponseDTO { Message = "Vehicle not found" });

                await _vehicleService.SetVehicleStatus(id, status);
                return Ok(new ResponseDTO { Message = "Status updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO { Message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpPatch("{id:int}/toggle")]
        public async Task<IActionResult> ToggleVehicleActiveStatus(int id)
        {
            try
            {
                await _vehicleService.ToggleActiveStatus(id);
                return Ok(new ResponseDTO { Message = "Vehicle active status toggled successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO { Message = ex.Message });
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
                    return NotFound(new ResponseDTO { Message = "Vehicle not found" });

                bool isAvailable = (vehicle.IsActive ?? false) && vehicle.Status == "Available";

                return Ok(new
                {
                    success = true,
                    isAvailable,
                    vehicle = new
                    {
                        vehicle.VehicleId,
                        vehicle.ModelId,
                        vehicle.Color,
                        vehicle.Status,
                        vehicle.IsActive
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO { Message = ex.Message });
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

                return Ok(new
                {
                    success = true,
                    count = available.Count,
                    vehicles = available
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO { Message = ex.Message });
            }
        }
    }
}
