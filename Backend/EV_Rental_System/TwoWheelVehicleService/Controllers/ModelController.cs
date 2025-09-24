using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TwoWheelVehicleService.DTOs;
using TwoWheelVehicleService.Services;

namespace TwoWheelVehicleService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModelController : ControllerBase
    {
        private readonly IModelService _modelService;

        public ModelController(IModelService modelService)
        {
            _modelService = modelService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllModels()
        {
            var models = await _modelService.GetAllModelsAsync();
            return Ok(models);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveModels()
        {
            var models = await _modelService.GetActiveModelsAsync();
            return Ok(models);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetModelById(int id)
        {
            var model = await _modelService.GetModelByIdAsync(id);
            if (model == null) return NotFound();
            return Ok(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateModel([FromForm] ModelRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await _modelService.AddModelAsync(request);
            return Ok(new { message = "Model created successfully" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateModel(int id, [FromForm] ModelRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                await _modelService.UpdateModelAsync(id, request);
                return Ok(new { message = "Model updated successfully" });
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteModel(int id)
        {
            var existingModel = await _modelService.GetModelByIdAsync(id);
            if (existingModel == null) return NotFound();

            await _modelService.DeleteModelAsync(id);
            return Ok(new { message = "Model deleted successfully" });
        }
    }
}