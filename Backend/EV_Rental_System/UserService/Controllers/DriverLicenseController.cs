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
        private readonly ILogger<DriverLicenseController> _logger;

        public DriverLicenseController(IDriverLicenseService driverLicenseService, IJwtService jwtService, ILogger<DriverLicenseController> logger)
        {
            _driverLicenseService = driverLicenseService;
            _jwtService = jwtService;
            _logger = logger;
        }

        // ============================================
        // 🔹 Helper: lấy UserId từ JWT
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
        // 🔹 Helper: trả về lỗi model
        // ============================================


        // ============================================
        // ✅ Member: gửi yêu cầu thêm bằng lái
        // ============================================
        [Authorize(Roles = "Member")]
        [HttpPost]
        public async Task<IActionResult> CreateDriverLicense([FromForm] DriverLicenseRequest request)
        {
            // ✅ Thêm logging chi tiết ngay đầu
            _logger.LogInformation("=== [CreateDriverLicense] ===");
            _logger.LogInformation("LicenseId: {LicenseId}", request.LicenseId);
            _logger.LogInformation("LicenseType: {LicenseType}", request.LicenseType);
            _logger.LogInformation("RegisterDate: {RegisterDate}", request.RegisterDate);
            _logger.LogInformation("RegisterOffice: {RegisterOffice}", request.RegisterOffice);
            _logger.LogInformation("DayOfBirth: {DayOfBirth}", request.DayOfBirth);
            _logger.LogInformation("FullName: {FullName}", request.FullName);
            _logger.LogInformation("Sex: {Sex}", request.Sex);
            _logger.LogInformation("Address: {Address}", request.Address);
            _logger.LogInformation("Files count: {FileCount}", request.Files?.Count ?? 0);

            if (request.Files != null)
            {
                foreach (var f in request.Files)
                {
                    _logger.LogInformation("File received: {FileName} ({Length} bytes)", f.FileName, f.Length);
                }
            }

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
                _logger.LogWarning("Invalid model state: {@Errors}", errors);
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
                _logger.LogInformation("UserId from token: {UserId}", userId);

                var result = await _driverLicenseService.AddDriverLicense(request, userId);

                _logger.LogInformation("Driver license created successfully for UserId {UserId}", userId);

                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Thêm bằng lái thành công",
                    Data = result
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access while creating driver license");
                return Unauthorized(new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument error while creating driver license");
                return BadRequest(new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating driver license");
                return StatusCode(500, new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Lỗi hệ thống nội bộ",
                    Data = ex.Message
                });
            }
        }

        // ============================================
        // ✅ Member: cập nhật lại bằng lái của mình
        // ============================================
        [Authorize(Roles = "Member")]
        [HttpPut]
        public async Task<IActionResult> UpdateDriverLicense([FromForm] DriverLicenseRequest request)
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

                _logger.LogWarning("Invalid model state: {@Errors}", errors);

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
                await _driverLicenseService.UpdateDriverLicense(request, userId);

                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Yêu cầu cập nhật bằng lái đã được gửi thành công"
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
        // ✅ Admin + Employee: xóa hồ sơ bằng lái
        // ============================================
        [Authorize(Roles = "Admin,Employee")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDriverLicense(int id)
        {
            try
            {
                await _driverLicenseService.DeleteDriverLicense(id);

                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Xóa hồ sơ bằng lái thành công"
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

        [HttpGet("{userId:int}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> SearchDriverLicenseByUserId(int userId)
        {
            try
            {
                var driverLicense = await _driverLicenseService.GetDriverLicenseByUserId(userId);
                if (driverLicense == null)
                {
                    return NotFound(new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Không tìm thấy hồ sơ bằng lái."
                    });
                }
                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Lấy thông tin bằng lái thành công",
                    Data = driverLicense
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

        [Authorize(Roles = "Member")]
        [HttpGet]
        public async Task<IActionResult> GetDriverLicenseByUserId()
        {
            try
            {
                int userId = GetUserIdFromToken();
                var driverLicense = await _driverLicenseService.GetDriverLicenseByUserId(userId);
                if (driverLicense == null)
                {
                    return NotFound(new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Không tìm thấy hồ sơ bằng lái."
                    });
                }

                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Lấy thông tin bằng lái thành công",
                    Data = driverLicense
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
        // ✅ Admin + Employee + Staff: duyệt hoặc từ chối hồ sơ
        // ============================================
        [Authorize(Roles = "Admin,Employee")]
        [HttpPost("set-status/{userId}/{isApproved}")]
        public async Task<IActionResult> SetStatus(int userId, bool isApproved)
        {
            try
            {
                var notification = await _driverLicenseService.SetStatus(userId, isApproved);

                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Cập nhật trạng thái hồ sơ bằng lái thành công",
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
