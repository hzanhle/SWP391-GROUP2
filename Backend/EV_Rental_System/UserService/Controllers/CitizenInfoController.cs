using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.DTOs;
using UserService.Services;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 🔒 Yêu cầu xác thực JWT
    public class CitizenInfoController : ControllerBase
    {
        private readonly ICitizenInfoService _citizenInfoService;
        private readonly IJwtService _jwtService;

        public CitizenInfoController(ICitizenInfoService citizenInfoService, IJwtService jwtService)
        {
            _citizenInfoService = citizenInfoService;
            _jwtService = jwtService;
        }

        // ============================================
        // 🔹 Helper: Lấy UserId từ JWT
        // ============================================
        private int GetUserIdFromToken()
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (string.IsNullOrEmpty(token))
                throw new UnauthorizedAccessException("Token không tồn tại.");

            var userId = _jwtService.GetUserIdFromToken(token);
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Không thể trích xuất UserId từ token.");

            return int.Parse(userId);
        }

        // ============================================
        // 🔹 Tạo thông tin CCCD
        // ============================================
        [HttpPost]
        public async Task<IActionResult> CreateCitizenInfo([FromForm] CitizenInfoRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Any())
                    .Select(x => new
                    {
                        Field = x.Key,
                        Errors = x.Value.Errors.Select(e => e.ErrorMessage)
                    })
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
                var userId = GetUserIdFromToken();
                var result = await _citizenInfoService.AddCitizenInfo(request, userId);

                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Tạo thông tin CCCD thành công",
                    Data = result
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message
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

        // ============================================
        // 🔹 Cập nhật thông tin CCCD
        // ============================================
        [HttpPut]
        public async Task<IActionResult> UpdateCitizenInfo([FromForm] CitizenInfoRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Any())
                    .Select(x => new
                    {
                        Field = x.Key,
                        Errors = x.Value.Errors.Select(e => e.ErrorMessage)
                    })
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
                var userId = GetUserIdFromToken();
                await _citizenInfoService.UpdateCitizenInfo(request, userId);

                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Yêu cầu cập nhật CCCD đã được gửi thành công"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message
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

        // ============================================
        // 🔹 Lấy thông tin CCCD theo UserId
        // ============================================
        [AllowAnonymous]
        [HttpGet("{userId:int}")]
        public async Task<IActionResult> GetCitizenInfoByUserId(int userId)
        {
            try
            {
                var citizenInfo = await _citizenInfoService.GetCitizenInfoByUserId(userId);
                if (citizenInfo == null)
                {
                    return NotFound(new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin CCCD cho user này."
                    });
                }

                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Lấy thông tin CCCD thành công",
                    Data = citizenInfo
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

        // ============================================
        // 🔹 Xóa thông tin CCCD
        // ============================================
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCitizenInfo(int id)
        {
            try
            {
                await _citizenInfoService.DeleteCitizenInfo(id);

                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Xóa thông tin CCCD thành công"
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

        // ============================================
        // 🔹 Duyệt hoặc từ chối CCCD
        // ============================================
        [Authorize(Roles = "Admin")]
        [HttpPost("status/{userId:int}/{isApproved:bool}")]
        public async Task<IActionResult> SetStatus(int userId, bool isApproved)
        {
            try
            {
                var notification = await _citizenInfoService.SetStatus(userId, isApproved);

                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Cập nhật trạng thái CCCD thành công",
                    Data = notification
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
