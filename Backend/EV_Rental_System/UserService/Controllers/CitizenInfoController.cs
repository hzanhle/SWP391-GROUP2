using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.DTOs;
using UserService.Services;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 🔒 Bắt buộc xác thực JWT cho toàn bộ controller
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
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ResponseDTO
                    {
                        Message = "Dữ liệu không hợp lệ",
                        Data = ModelState
                            .Where(x => x.Value.Errors.Any())
                            .Select(x => new
                            {
                                Field = x.Key,
                                Errors = x.Value.Errors.Select(e => e.ErrorMessage)
                            })
                            .ToList()
                    });

                var userId = GetUserIdFromToken();
                var response = await _citizenInfoService.AddCitizenInfo(request, userId);

                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi hệ thống nội bộ", details = ex.Message });
            }
        }

        // ============================================
        // 🔹 Cập nhật thông tin CCCD
        // ============================================
        [HttpPut]
        public async Task<IActionResult> UpdateCitizenInfo([FromForm] CitizenInfoRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ResponseDTO
                    {
                        Message = "Dữ liệu không hợp lệ",
                        Data = ModelState
                            .Where(x => x.Value.Errors.Any())
                            .Select(x => new
                            {
                                Field = x.Key,
                                Errors = x.Value.Errors.Select(e => e.ErrorMessage)
                            })
                            .ToList()
                    });

                var userId = GetUserIdFromToken();
                await _citizenInfoService.UpdateCitizenInfo(request, userId);

                return Ok(new { message = "Yêu cầu cập nhật CCCD đã được gửi thành công." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        // ============================================
        // 🔹 Lấy thông tin CCCD theo UserId
        // ============================================
        [AllowAnonymous] // 🟡 Cho phép admin hoặc người khác truy cập mà không cần token
        [HttpGet("{userId:int}")]
        public async Task<IActionResult> GetCitizenInfoByUserId(int userId)
        {
            try
            {
                var citizenInfo = await _citizenInfoService.GetCitizenInfoByUserId(userId);
                if (citizenInfo == null)
                    return NotFound(new { message = "Citizen info not found." });

                return Ok(citizenInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
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
                return Ok(new { message = "Citizen info deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }

        // ============================================
        // 🔹 Duyệt hoặc từ chối CCCD
        // ============================================
        [Authorize(Roles = "Admin")] // 🔒 Chỉ admin mới được duyệt
        [HttpPost("status/{userId:int}/{isApproved:bool}")]
        public async Task<IActionResult> SetStatus(int userId, bool isApproved)
        {
            try
            {
                var notification = await _citizenInfoService.SetStatus(userId, isApproved);
                return Ok(notification);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }
    }
}
