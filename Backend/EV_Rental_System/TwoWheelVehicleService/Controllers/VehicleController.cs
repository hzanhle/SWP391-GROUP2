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
            var model = await _modelService.GetModelByIdAsync(vehicle.VehicleId);
            vehicle.Model = model;

            return Ok(vehicle);
        }


        [HttpPost]
        public async Task<IActionResult> CreateVehicle([FromBody] VehicleRequest vehicle)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await _vehicleService.AddVehicleAsync(vehicle);
            return Ok(new { message = "Vehicle created successfully" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] Vehicle vehicle)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

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
    }
}