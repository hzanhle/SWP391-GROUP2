using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
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

        [HttpPost("register/send-otp")] // Gửi OTP khi đăng ký
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

                    return BadRequest(new ResponseDTO
                    {
                        Message = "Dữ liệu không hợp lệ",
                        Data = errors
                    });
                }

                var result = await _otpService.SendRegistrationOtpAsync(
                    registerRequest.Email,
                    registerRequest
                );

                if (!result.Success == true)
                {
                    return BadRequest(new ResponseDTO
                    {
                        Message = result.Message
                    });
                }

                return Ok(new ResponseDTO
                {
                    Message = result.Message,
                    Data = new { email = registerRequest.Email }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending registration OTP");
                return StatusCode(500, new ResponseDTO
                {
                    Message = "Đã xảy ra lỗi khi gửi OTP"
                });
            }
        }

        [HttpPost("register/verify-otp")]
        public async Task<IActionResult> VerifyRegistrationOtp([FromBody] string email, string otp)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ResponseDTO
                    {
                        Message = "Dữ liệu không hợp lệ"
                    });
                }

                // Xác thực OTP
                var otpResult = await _otpService.VerifyRegistrationOtpAsync(email, otp);

                if (!otpResult.Success == true)
                {
                    return BadRequest(new ResponseDTO
                    {
                        Message = otpResult.Message
                    });
                }

                // Deserialize thông tin đăng ký từ cache
                var registerData = JsonSerializer.Deserialize<RegisterRequestDTO>(
                    otpResult.Data,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (registerData == null)
                {
                    return BadRequest(new ResponseDTO
                    {
                        Message = "Không thể lấy thông tin đăng ký"
                    });
                }

                // Tạo user
                var user = await _userService.CreateUserFromRegistrationAsync(registerData);

                return Ok(new ResponseDTO
                {
                    Message = "Đăng ký tài khoản thành công",
                    Data = new
                    {
                        userId = user.Id,
                        userName = user.UserName,
                        email = user.Email
                    }
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ResponseDTO
                {
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying registration OTP");
                return StatusCode(500, new ResponseDTO
                {
                    Message = "Đã xảy ra lỗi khi xác thực OTP"
                });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] User user)
        {
            try
            {
                // Kiểm tra ModelState
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

        [HttpPatch("{userId}")]
        public async Task<IActionResult> SetActive(int userId)
        {
            try
            {
                await _userService.SetStatus(userId);
                return Ok(new { message = "User status toggled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status for user {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPatch("SetRole{id}")]
        public async Task<IActionResult> SetAdmin(int id)
        {
            try
            {
                await _userService.SetAdmin(id);
                return Ok(new { message = "Admin set successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting admin role for user {UserId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("GetStaffAccounts")]
        public async Task<IActionResult> GetStaffAccount()
        {
            try
            {
                var result = await _userService.GetAllStaffAccount();
                return Ok(result);
            } catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("AddStaffAccount")]
        public async Task<IActionResult> AddStaffAccount([FromBody] StaffDTO staff)
        {
            try
            {
                // Kiểm tra ModelState
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
                await _userService.AddStaffAsync(staff);
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
                // Kiểm tra ModelState
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
                var result = await _userService.ChangePassword(passwordRequest);

                if (result.IsSuccess)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] DTOs.LoginRequest loginRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ResponseDTO
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
                return StatusCode(500, new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau."
                });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { message = "Email không được để trống" });

            var result = await _otpService.SendPasswordResetOtpAsync(email);
            return Ok(result);
        }

        [HttpPost("verify-reset-otp")]
        public async Task<IActionResult> VerifyResetOTP([FromBody] string otp, string email)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otp))
                return BadRequest(new { message = "Email và OTP không được để trống" });

            var result = await _otpService.VerifyPasswordResetOtpAsync(email,otp);

            if (result.Success == true)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] DTOs.ResetPasswordRequest request)
        {
            // Kiểm tra ModelState
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
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { message = "Email không được để trống" });

            if (string.IsNullOrWhiteSpace(request.Otp))
                return BadRequest(new { message = "Mã OTP không được để trống" });

            var result = await _userService.ResetPasswordAsync(request);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpGet("staff")]
        public async Task<IActionResult> GetStaffUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                var staffUsers = users.Where(u => u.RoleId == 2).ToList(); // Lấy danh sách nhân viên (RoleId = 2)
                return Ok(staffUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting staff users");
                return StatusCode(500, "Internal server error");
            }
        }

    }
}
