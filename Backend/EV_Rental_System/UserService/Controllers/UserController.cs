using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .SelectMany(x => x.Value.Errors)
                        .Select(x => x.ErrorMessage)
                        .ToList();

                    return BadRequest(new LoginResponse
                    {
                        IsSuccess = false,
                        Message = "Dữ liệu không hợp lệ"
                    });
                }

                var result = await _userService.Login(loginRequest);

                if (!result.IsSuccess)
                {
                    if (result.Message.Contains("mật khẩu") || result.Message.Contains("khóa"))
                    {
                        return Unauthorized(result);
                    }
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login");
                return StatusCode(500, new LoginResponse
                {
                    IsSuccess = false,
                    Message = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau."
                });
            }
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound();
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("UserDetail{userId}")]
        public async Task<IActionResult> GetUserDetail(int userId)
        {
            try
            {
                var user = await _userService.GetUserDetailByIdAsync(userId);
                if (user == null)
                {
                    return NotFound();
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user detail {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            try
            {
                await _userService.AddUserAsync(user);
                return Ok(new { message = "User registered successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] User user)
        {
            try
            {
                await _userService.UpdateUserAsync(user);
                return Ok(new { message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser([FromQuery] int userId)
        {
            try
            {
                await _userService.DeleteUserAsync(userId);
                return Ok(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> SetAdmin(int id)
        {
            try
            {
                await _userService.SetRole(id);
                return Ok(new { message = "Admin set successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting admin role for user {UserId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("AddStaffAccount")]
        public async Task<IActionResult> AddStaffAccount([FromBody] User user)
        {
            try
            {
                await _userService.AddStaffAccount(user);
                return Ok(new { message = "Staff account created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding staff account");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest passwordRequest)
        {
            try
            {
                await _userService.ChangePassword(passwordRequest);
                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, "Internal server error");
            }
        }

        // ============================================
        // OTP ENDPOINTS - ĐƠN GIẢN
        // ============================================

        /// <summary>
        /// Gửi OTP đến email
        /// </summary>
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOTP([FromBody] string email)
        {
            var result = await _otpService.SendOtpAsync(email);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Xác thực OTP
        /// </summary>
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOTP([FromBody] OtpResponse request)
        {
            var result = await _otpService.VerifyOtpAsync(request.Email, request.Otp);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }
    }

    

    

    
}