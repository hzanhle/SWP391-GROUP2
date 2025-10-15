using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.DTOs;
using UserService.Services;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // ✅ yêu cầu xác thực cho tất cả endpoint
    public class DriverLicenseController : ControllerBase
    {
        private readonly IDriverLicenseService _driverLicenseService;
        private readonly IJwtService _jwtService;

        public DriverLicenseController(IDriverLicenseService driverLicenseService, IJwtService jwtService)
        {
            _driverLicenseService = driverLicenseService;
            _jwtService = jwtService;
        }

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

        private IActionResult HandleInvalidModel()
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

        // ✅ Member: gửi yêu cầu thêm bằng lái
        [Authorize(Roles = "Member")]
        [HttpPost]
        public async Task<IActionResult> CreateDriverLicense([FromForm] DriverLicenseRequest request)
        {
            if (!ModelState.IsValid)
                return HandleInvalidModel();

            try
            {
                var userId = GetUserIdFromToken();
                var response = await _driverLicenseService.AddDriverLicense(request, userId);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error.", details = ex.Message });
            }
        }

        // ✅ Member: cập nhật lại bằng lái của mình
        [Authorize(Roles = "Member")]
        [HttpPut]
        public async Task<IActionResult> UpdateDriverLicense([FromForm] DriverLicenseRequest request)
        {
            if (!ModelState.IsValid)
                return HandleInvalidModel();

            try
            {
                var userId = GetUserIdFromToken();
                await _driverLicenseService.UpdateDriverLicense(request, userId);
                return Ok(new { message = "Driver license update request sent successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }

        // ✅ Admin + Employee: xóa hồ sơ bằng lái
        [Authorize(Roles = "Admin,Employee")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDriverLicense(int id)
        {
            try
            {
                await _driverLicenseService.DeleteDriverLicense(id);
                return Ok(new { message = "Driver license deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }

        // ✅ Admin + Employee + Member: xem hồ sơ
        [Authorize(Roles = "Admin,Employee,Member")]
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetDriverLicenseByUserId(int userId)
        {
            try
            {
                var driverLicense = await _driverLicenseService.GetDriverLicenseByUserId(userId);
                if (driverLicense == null)
                    return NotFound(new { message = "Driver license not found." });

                return Ok(driverLicense);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }

        // ✅ Admin + Employee: duyệt hoặc từ chối hồ sơ
        [Authorize(Roles = "Admin,Employee")]
        [HttpPost("set-status/{userId}/{isApproved}")]
        public async Task<IActionResult> SetStatus(int userId, bool isApproved)
        {
            try
            {
                var notification = await _driverLicenseService.SetStatus(userId, isApproved);
                return Ok(notification);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }
    }
}
