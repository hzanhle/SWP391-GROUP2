using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using UserService.DTOs;
using UserService.Models;
using UserService.Services;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IOtpService _otpService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, IOtpService otpService, ILogger<UserController> logger)
        {
            _userService = userService;
            _otpService = otpService;
            _logger = logger;
        }

        // -------------------- USERS --------------------
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(new ResponseDTO { IsSuccess = true, Message = "Lấy danh sách người dùng thành công", Data = users });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return StatusCode(500, new ResponseDTO { IsSuccess = false, Message = ex.Message, Data = ex });
            }
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                    return NotFound(new ResponseDTO { IsSuccess = false, Message = "Không tìm thấy người dùng" });

                return Ok(new ResponseDTO { IsSuccess = true, Message = "Lấy thông tin người dùng thành công", Data = user });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {UserId}", userId);
                return StatusCode(500, new ResponseDTO { IsSuccess = false, Message = ex.Message, Data = ex });
            }
        }

        [HttpGet("UserDetail/{userId}")]
        public async Task<IActionResult> GetUserDetail(int userId)
        {
            try
            {
                var user = await _userService.GetUserDetailByIdAsync(userId);
                if (user == null)
                    return NotFound(new ResponseDTO { IsSuccess = false, Message = "Không tìm thấy thông tin chi tiết người dùng" });

                return Ok(new ResponseDTO { IsSuccess = true, Message = "Lấy chi tiết người dùng thành công", Data = user });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user detail {UserId}", userId);
                return StatusCode(500, new ResponseDTO { IsSuccess = false, Message = ex.Message, Data = ex });
            }
        }

        // -------------------- REGISTER / OTP --------------------
        [HttpPost("register/send-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> SendRegistrationOtp([FromBody] RegisterRequestDTO registerRequest)
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

                    return BadRequest(new ResponseDTO { IsSuccess = false, Message = "Dữ liệu không hợp lệ", Data = errors });
                }

                var result = await _otpService.SendRegistrationOtpAsync(registerRequest.Email, registerRequest);

                if (result.Success == false)
                    return BadRequest(new ResponseDTO { IsSuccess = false, Message = result.Message, Data = result });

                return Ok(new ResponseDTO { IsSuccess = true, Message = result.Message, Data = new { email = registerRequest.Email } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending registration OTP");
                return StatusCode(500, new ResponseDTO { IsSuccess = false, Message = ex.Message, Data = ex });
            }
        }

        [HttpPost("register/verify-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyRegistrationOtp([FromBody] OtpAttribute request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Otp))
                    return BadRequest(new ResponseDTO { IsSuccess = false, Message = "Email hoặc OTP không hợp lệ" });

                var otpResult = await _otpService.VerifyRegistrationOtpAsync(request.Email, request.Otp);
                if (otpResult.Success == false)
                    return BadRequest(new ResponseDTO { IsSuccess = false, Message = otpResult.Message, Data = otpResult });

                var registerData = JsonSerializer.Deserialize<RegisterRequestDTO>(
                    otpResult.Data,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (registerData == null)
                    return BadRequest(new ResponseDTO { IsSuccess = false, Message = "Không thể lấy thông tin đăng ký từ cache" });

                var user = await _userService.CreateUserFromRegistrationAsync(registerData);

                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Đăng ký tài khoản thành công",
                    Data = new { user.Id, user.UserName, user.Email }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying registration OTP");
                return StatusCode(500, new ResponseDTO { IsSuccess = false, Message = ex.Message, Data = ex });
            }
        }

        // -------------------- UPDATE / DELETE --------------------
        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] User user)
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

                    return BadRequest(new ResponseDTO { IsSuccess = false, Message = "Dữ liệu không hợp lệ", Data = errors });
                }

                await _userService.UpdateUserAsync(user);
                return Ok(new ResponseDTO { IsSuccess = true, Message = "Cập nhật người dùng thành công", Data = user });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user");
                return StatusCode(500, new ResponseDTO { IsSuccess = false, Message = ex.Message, Data = ex });
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> DeleteUser([FromQuery] int userId)
        {
            try
            {
                await _userService.DeleteUserAsync(userId);
                return Ok(new ResponseDTO { IsSuccess = true, Message = "Xóa người dùng thành công", Data = new { userId } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", userId);
                return StatusCode(500, new ResponseDTO { IsSuccess = false, Message = ex.Message, Data = ex });
            }
        }

        // -------------------- ROLE / STATUS --------------------
        [HttpPatch("{userId}/toggle-active")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> SetActive(int userId)
        {
            try
            {
                await _userService.SetStatus(userId);
                return Ok(new ResponseDTO { IsSuccess = true, Message = "Đã thay đổi trạng thái người dùng", Data = new { userId } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status for user {UserId}", userId);
                return StatusCode(500, new ResponseDTO { IsSuccess = false, Message = ex.Message, Data = ex });
            }
        }

        [HttpPatch("SetRole/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetAdmin(int id)
        {
            try
            {
                await _userService.SetAdmin(id);
                return Ok(new ResponseDTO { IsSuccess = true, Message = "Cập nhật quyền Admin thành công", Data = new { id } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting admin role for user {UserId}", id);
                return StatusCode(500, new ResponseDTO { IsSuccess = false, Message = ex.Message, Data = ex });
            }
        }

        // -------------------- STAFF --------------------
        [HttpGet("GetStaffAccounts")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetStaffAccount()
        {
            try
            {
                var result = await _userService.GetAllStaffAccount();
                return Ok(new ResponseDTO { IsSuccess = true, Message = "Lấy danh sách nhân viên thành công", Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting staff accounts");
                return StatusCode(500, new ResponseDTO { IsSuccess = false, Message = ex.Message, Data = ex });
            }
        }

        [HttpPost("AddStaffAccount")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddStaffAccount([FromBody] StaffDTO staff)
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

                    return BadRequest(new ResponseDTO { IsSuccess = false, Message = "Dữ liệu không hợp lệ", Data = errors });
                }

                await _userService.AddStaffAsync(staff);
                return Ok(new ResponseDTO { IsSuccess = true, Message = "Tạo tài khoản nhân viên thành công", Data = staff });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding staff account");
                return StatusCode(500, new ResponseDTO { IsSuccess = false, Message = ex.Message, Data = ex });
            }
        }

        // -------------------- AUTH --------------------
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ResponseDTO { IsSuccess = false, Message = "Dữ liệu không hợp lệ" });

                var result = await _userService.Login(loginRequest);
                return result.IsSuccess
                    ? Ok(result)
                    : Unauthorized(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login");
                return StatusCode(500, new ResponseDTO { IsSuccess = false, Message = ex.Message, Data = ex });
            }
        }

        [HttpPost("ChangePassword")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest passwordRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ResponseDTO { IsSuccess = false, Message = "Dữ liệu không hợp lệ", Data = ModelState });

                var result = await _userService.ChangePassword(passwordRequest);
                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, new ResponseDTO { IsSuccess = false, Message = ex.Message, Data = ex });
            }
        }

        // -------------------- PASSWORD RESET --------------------
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return BadRequest(new ResponseDTO { IsSuccess = false, Message = "Email không được để trống" });

                var result = await _otpService.SendPasswordResetOtpAsync(email);
                return Ok(new ResponseDTO { IsSuccess = true, Message = result.Message, Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending forgot-password OTP");
                return StatusCode(500, new ResponseDTO { IsSuccess = false, Message = ex.Message, Data = ex });
            }
        }

        [HttpPost("verify-reset-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyResetOTP([FromBody] OtpAttribute request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Otp))
                    return BadRequest(new ResponseDTO { IsSuccess = false, Message = "Email hoặc OTP không được để trống" });

                var result = await _otpService.VerifyPasswordResetOtpAsync(request.Email, request.Otp);
                return Ok(new ResponseDTO { IsSuccess = true, Message = result.Message, Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying reset OTP");
                return StatusCode(500, new ResponseDTO { IsSuccess = false, Message = ex.Message, Data = ex });
            }
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ResponseDTO { IsSuccess = false, Message = "Dữ liệu không hợp lệ", Data = ModelState });

                var result = await _userService.ResetPasswordAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password");
                return StatusCode(500, new ResponseDTO { IsSuccess = false, Message = ex.Message, Data = ex });
            }
        }
    }
}
