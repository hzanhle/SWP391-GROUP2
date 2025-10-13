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
            try
            {
                var models = await _modelService.GetAllModelsAsync();
                return Ok(models);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO { Message = ex.Message });
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveModels()
        {
            try
            {
                var models = await _modelService.GetActiveModelsAsync();
                return Ok(models);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO { Message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetModelById(int id)
        {
            try
            {
                var model = await _modelService.GetModelByIdAsync(id);
                if (model == null)
                    return NotFound(new ResponseDTO { Message = "Model not found" });

                return Ok(model);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO { Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateModel([FromForm] ModelRequest request)
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

                await _modelService.AddModelAsync(request);
                return Ok(new ResponseDTO { Message = "Model created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO { Message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateModel(int id, [FromForm] ModelRequest request)
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

                await _modelService.UpdateModelAsync(id, request);
                return Ok(new ResponseDTO { Message = "Model updated successfully" });
            }
            catch (ArgumentException)
            {
                return NotFound(new ResponseDTO { Message = "Model not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO { Message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteModel(int id)
        {
            try
            {
                var existingModel = await _modelService.GetModelByIdAsync(id);
                if (existingModel == null)
                    return NotFound(new ResponseDTO { Message = "Model not found" });

                await _modelService.DeleteModelAsync(id);
                return Ok(new ResponseDTO { Message = "Model deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO { Message = ex.Message });
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            try
            {
                await _modelService.ToggleStatusAsync(id);
                return Ok(new ResponseDTO { Message = "Model status changed successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO { Message = ex.Message });
            }
        }
    }
}
