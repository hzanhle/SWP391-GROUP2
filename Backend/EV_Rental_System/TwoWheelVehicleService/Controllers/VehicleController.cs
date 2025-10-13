using Microsoft.AspNetCore.Mvc;
using TwoWheelVehicleService.Services;
using TwoWheelVehicleService.Models;
using TwoWheelVehicleService.DTOs;

namespace TwoWheelVehicleService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehicleController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [HttpGet]
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

        [HttpGet("{id}")]
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

        [HttpPost]
        public async Task<IActionResult> CreateVehicle([FromBody] VehicleRequest vehicle)
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
                        Message = "Dữ liệu không hợp lệ",
                        Data = errors
                    });
                }

                await _vehicleService.AddVehicleAsync(vehicle);
                return Ok(new ResponseDTO { Message = "Vehicle created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO { Message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] Vehicle vehicle)
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
                        Message = "Dữ liệu không hợp lệ",
                        Data = errors
                    });
                }

                var existingVehicle = await _vehicleService.GetVehicleByIdAsync(id);
                if (existingVehicle == null)
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            try
            {
                var existingVehicle = await _vehicleService.GetVehicleByIdAsync(id);
                if (existingVehicle == null)
                    return NotFound(new ResponseDTO { Message = "Vehicle not found" });

                await _vehicleService.DeleteVehicleAsync(id);
                return Ok(new ResponseDTO { Message = "Vehicle deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO { Message = ex.Message });
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateVehicleStatus(int id, [FromBody] string status)
        {
            try
            {
                if (string.IsNullOrEmpty(status))
                    return BadRequest(new ResponseDTO { Message = "Status is required" });

                var existingVehicle = await _vehicleService.GetVehicleByIdAsync(id);
                if (existingVehicle == null)
                    return NotFound(new ResponseDTO { Message = "Vehicle not found" });

                await _vehicleService.SetVehicleStatus(id, status);
                return Ok(new ResponseDTO { Message = "Status updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO { Message = ex.Message });
            }
        }

        [HttpPatch("{id}")]
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

        /// <summary>
        /// Check vehicle availability for booking (called by BookingService)
        /// </summary>
        [HttpPost("check-availability")]
        public async Task<IActionResult> CheckAvailabilityByVehicleId([FromBody] int vehicleId)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var vehicle = await _vehicleService.GetVehicleByIdAsync(vehicleId);
                if (vehicle == null)
                    return NotFound(new ResponseDTO { Message = "Vehicle not found" });

                bool isAvailable = vehicle.IsActive == true && vehicle.Status == "Available";

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

        /// <summary>
        /// Get available vehicles by model for specific dates
        /// </summary>
        [HttpPost("available-by-model")]
        public async Task<IActionResult> GetAvailableVehiclesByModel([FromBody] int modelId)
        {
            try
            {
                var allVehicles = await _vehicleService.GetAllVehiclesAsync();

                var availableVehicles = allVehicles
                    .Where(v => v.ModelId == modelId
                             && v.IsActive
                             && v.Status == "Available")
                    .ToList();

                return Ok(new
                {
                    success = true,
                    count = availableVehicles.Count,
                    vehicles = availableVehicles
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO { Message = ex.Message });
            }
        }
    }
}
