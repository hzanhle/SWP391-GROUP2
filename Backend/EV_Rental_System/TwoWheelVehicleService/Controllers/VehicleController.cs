using Microsoft.AspNetCore.Http;
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
        private readonly IModelService _modelService;

        public VehicleController(IVehicleService vehicleService, IModelService modelService)
        {
            _vehicleService = vehicleService;
            _modelService = modelService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllVehicles()
        {
            var vehicles = await _vehicleService.GetAllVehiclesAsync();
            return Ok(vehicles);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveVehicles()
        {
            var vehicles = await _vehicleService.GetActiveVehiclesAsync();
            return Ok(vehicles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicleById(int id)
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
            if (vehicle == null) return NotFound();
            return Ok(vehicle);
        }


        [HttpPost]
        public async Task<IActionResult> CreateVehicle([FromBody] VehicleRequest vehicle)
        {
            // Kiểm tra ModelState
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
            return Ok(new { message = "Vehicle created successfully" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] Vehicle vehicle)
        {
            // Kiểm tra ModelState
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
            if (existingVehicle == null) return NotFound();

            vehicle.VehicleId = id;
            await _vehicleService.UpdateVehicleAsync(vehicle);
            return Ok(new { message = "Vehicle updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            var existingVehicle = await _vehicleService.GetVehicleByIdAsync(id);
            if (existingVehicle == null) return NotFound();

            await _vehicleService.DeleteVehicleAsync(id);
            return Ok(new { message = "Vehicle deleted successfully" });
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateVehicleStatus(int id, [FromBody] string status)
        {
            if (string.IsNullOrEmpty(status)) return BadRequest("Status is required");

            var existingVehicle = await _vehicleService.GetVehicleByIdAsync(id);
            if (existingVehicle == null) return NotFound();

            await _vehicleService.SetVehicleStatus(id, status);
            return Ok(new { message = "Status updated successfully" });
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
                    return NotFound(new { success = false, message = "Vehicle not found" });

                // Check if vehicle is active and available
                // Using == true to handle nullable bool
                bool isAvailable = vehicle.IsActive == true && vehicle.Status == "Available";

                return Ok(new
                {
                    success = true,
                    isAvailable = isAvailable,
                    vehicle = new
                    {
                        vehicleId = vehicle.VehicleId,
                        modelId = vehicle.ModelId,
                        color = vehicle.Color,
                        status = vehicle.Status,
                        isActive = vehicle.IsActive
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
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
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }

    
}
