using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwoWheelVehicleService.DTOs;
using TwoWheelVehicleService.Services;

namespace TwoWheelVehicleService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 🔒 Chỉ cho phép người đăng nhập sử dụng
    public class ModelController : ControllerBase
    {
        private readonly IModelService _modelService;

        public ModelController(IModelService modelService)
        {
            _modelService = modelService;
        }

        // ============================ GET ============================

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllModels()
        {
            try
            {
                var models = await _modelService.GetAllModelsAsync();
                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Lấy danh sách model thành công",
                    Data = models
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Lỗi hệ thống nội bộ",
                    Data = ex.Message
                });
            }
        }

        [AllowAnonymous]
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveModels()
        {
            try
            {
                var models = await _modelService.GetActiveModelsAsync();
                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Lấy danh sách model active thành công",
                    Data = models
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Lỗi hệ thống nội bộ",
                    Data = ex.Message
                });
            }
        }

        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetModelById(int id)
        {
            try
            {
                var model = await _modelService.GetModelByIdAsync(id);
                if (model == null)
                    return NotFound(new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Model không tồn tại"
                    });

                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Lấy model thành công",
                    Data = model
                });
            }
            catch (Exception ex)
            {
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
        public async Task<IActionResult> CreateModel([FromForm] ModelRequest request)
        {
            if (!ModelState.IsValid)
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

            try
            {
                await _modelService.AddModelAsync(request);
                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Tạo model thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Lỗi hệ thống nội bộ",
                    Data = ex.Message
                });
            }
        }

        // ============================ UPDATE ============================

        [Authorize(Roles = "Admin,Staff")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateModel(int id, [FromForm] ModelRequest request)
        {
            try
            {
                await _modelService.UpdateModelAsync(id, request);
                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Cập nhật model thành công"
                });
            }
            catch (ArgumentException)
            {
                return NotFound(new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Model không tồn tại"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Lỗi hệ thống nội bộ",
                    Data = ex.Message
                });
            }
        }

        // ============================ DELETE ============================

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteModel(int id)
        {
            try
            {
                var existing = await _modelService.GetModelByIdAsync(id);
                if (existing == null)
                    return NotFound(new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Model không tồn tại"
                    });

                await _modelService.DeleteModelAsync(id);
                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Xóa model thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Lỗi hệ thống nội bộ",
                    Data = ex.Message
                });
            }
        }

        // ============================ PATCH ============================

        [Authorize(Roles = "Admin,Staff")]
        [HttpPatch("{id:int}/toggle")]
        public async Task<IActionResult> ToggleModelStatus(int id)
        {
            try
            {
                await _modelService.ToggleStatusAsync(id);
                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Đổi trạng thái model thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Lỗi hệ thống nội bộ",
                    Data = ex.Message
                });
            }
        }
    }
}
