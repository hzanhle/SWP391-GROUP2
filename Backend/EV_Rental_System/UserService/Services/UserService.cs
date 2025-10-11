using BCrypt.Net;
using UserService.DTOs;
using UserService.Models;
using UserService.Repositories;

namespace UserService.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ICitizenInfoRepository _citizenInfoRepository;
        private readonly ILogger<UserService> _logger;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;
        private readonly IOtpService _otpService;


        public UserService(
            IRoleRepository roleRepository,
            IUserRepository userRepository,
            ICitizenInfoRepository citizenInfoRepository,
            ILogger<UserService> logger,
            IJwtService jwtService,
            IConfiguration configuration,
            IOtpService otpService)
        {
            _userRepository = userRepository;
            _citizenInfoRepository = citizenInfoRepository;
            _logger = logger;
            _jwtService = jwtService;
            _configuration = configuration;
            _roleRepository = roleRepository;
            _otpService = otpService;
        }

        public async Task<LoginResponse> Login(LoginRequest loginRequest)
        {
            try
            {
                // Input validation
                if (string.IsNullOrWhiteSpace(loginRequest.UserName) ||
                    string.IsNullOrWhiteSpace(loginRequest.Password))
                {
                    return new LoginResponse
                    {
                        IsSuccess = false,
                        Message = "Tên đăng nhập và mật khẩu không được để trống"
                    };
                }

                // Get user by username only (fixed - only pass username)
                var user = await _userRepository.GetUserAsync(loginRequest.UserName);
                var role = await _roleRepository.GetRoleByIdAsync(user.RoleId);

                if (user == null)
                {
                    _logger.LogWarning("Login failed - user not found: {Username}", loginRequest.UserName);
                    return new LoginResponse
                    {
                        IsSuccess = false,
                        Message = "Tên đăng nhập hoặc mật khẩu không đúng"
                    };
                }

                // Verify password using BCrypt
                if (!BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.Password))
                {
                    _logger.LogWarning("Login failed - invalid password for user: {UserId}", user.Id);
                    return new LoginResponse
                    {
                        IsSuccess = false,
                        Message = "Tên đăng nhập hoặc mật khẩu không đúng"
                    };
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    _logger.LogWarning("Login attempt for inactive user: {UserId}", user.Id);
                    return new LoginResponse
                    {
                        IsSuccess = false,
                        Message = "Tài khoản đã bị khóa"
                    };
                }

                // Generate JWT token using injected service
                var token = _jwtService.GenerateToken(user);

                _logger.LogInformation("User logged in successfully: {UserId}", user.Id);

                // Get citizen info safely
                var citizenInfo = await _citizenInfoRepository.GetCitizenInfoByUserId(user.Id);

                return new LoginResponse
                {
                    IsSuccess = true,
                    Message = "Đăng nhập thành công",
                    Token = token,
                    TokenType = "Bearer",
                    ExpiresIn = _configuration.GetSection("JwtSettings").GetValue<int>("ExpiresInMinutes") * 60,
                    User = new UserDTO
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        FullName = citizenInfo?.FullName, // Handle null case
                        RoleId = user.RoleId,
                        RoleName = role?.RoleName,
                        IsActive = user.IsActive,
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {Username}", loginRequest.UserName);
                return new LoginResponse
                {
                    IsSuccess = false,
                    Message = "Đã xảy ra lỗi trong quá trình đăng nhập"
                };
            }
        }
        public async Task AddUserAsync(User user)
        {
            try
            {
                var existingUser = await _userRepository.GetUserAsync(user.UserName);
                if (existingUser != null)
                {
                    _logger.LogWarning("Attempted to add user with existing username: {UserName}", user.UserName);
                    throw new ArgumentException("Tên đăng nhập đã tồn tại");
                }
                // Hash password before saving
                if (!string.IsNullOrEmpty(user.Password))
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                }
                var role = await _roleRepository.GetRoleByNameAsync("Member"); // Default role to "Member"
                user.CreatedAt = DateTime.UtcNow;
                user.IsActive = true; // Set default active status
                user.RoleId = role.RoleId;
                user.Role = role;

                await _userRepository.AddUserAsync(user);
                _logger.LogInformation("User added successfully: {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user: {UserName}", user.UserName);
                throw;
            }
        }

        public async Task AddStaffAsync(User user)
        {
            try
            {
                var existingUser = await _userRepository.GetUserAsync(user.UserName);
                if (existingUser != null)
                {
                    _logger.LogWarning("Attempted to add staff with existing username: {UserName}", user.UserName);
                    throw new ArgumentException("Tên đăng nhập đã tồn tại");
                }
                // Hash password before saving
                if (!string.IsNullOrEmpty(user.Password))
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                }
                var role = await _roleRepository.GetRoleByNameAsync("Employee"); // Default role to "Employee" for staff
                user.CreatedAt = DateTime.UtcNow;
                user.IsActive = true; // Set default active status
                user.RoleId = role.RoleId;
                user.Role = role;
                await _userRepository.AddUserAsync(user);
                _logger.LogInformation("Staff user added successfully: {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding staff user: {UserName}", user.UserName);
                throw;
            }
        }

        public async Task SetStatus(int userId)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user != null)
                {
                    if (user.IsActive == true)
                    {
                        user.IsActive = false;
                    }
                    else
                    {
                        user.IsActive = true;
                    }
                    await _userRepository.UpdateUserAsync(user);
                    _logger.LogInformation("User change status: {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("Attempted to delete non-existent user: {UserId}", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactive user: {UserId}", userId);
                throw;
            }
        }
        public async Task DeleteUserAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user != null)
                {
                    _userRepository.DeleteUserAsync(user);
                }
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", userId);
            }
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            try
            {
                var result = await _userRepository.GetAllUsersAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                throw;
            }
        }
        public async Task<User?> GetUserAsync(string userName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userName))
                    return null;

                return await _userRepository.GetUserAsync(userName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by username: {UserName}", userName);
                throw;
            }
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            try
            {
                return await _userRepository.GetUserByIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<User?> GetUserDetailByIdAsync(int userId)
        {
            try
            {
                return await _userRepository.GetUserDetailByIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user detail by ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<List<User>?> SearchUserAsync(string searchValue)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchValue))
                    return await _userRepository.GetAllUsersAsync();

                // Implement actual search logic
                return await _userRepository.SearchUserAsync(searchValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users with value: {SearchValue}", searchValue);
                throw;
            }
        }

        public async Task UpdateUserAsync(User user)
        {
            try
            {
                var existingUser = await _userRepository.GetUserByIdAsync(user.Id);
                if (existingUser == null)
                {
                    _logger.LogWarning("Attempted to update non-existent user: {UserId}", user.Id);
                    throw new ArgumentException("User not found");
                }

                existingUser.UserName = user.UserName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;


                // Only update password if provided and different
                if (!string.IsNullOrEmpty(user.Password) && user.Password != existingUser.Password)
                {
                    existingUser.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                }

                await _userRepository.UpdateUserAsync(existingUser);
                _logger.LogInformation("User updated successfully: {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", user.Id);
                throw;
            }
        }

        public async Task SetAdmin(int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", userId);
                return;
            }

            // Chuyển đổi RoleId
            if (user.RoleId == 2)
            {
                user.RoleId = 3; // Employee -> Admin
            }
            else if (user.RoleId == 3)
            {
                user.RoleId = 2; // Admin -> Employee
            }
            else
            {
                _logger.LogWarning("User {UserId} has unsupported RoleId {RoleId}.", user.Id, user.RoleId);
                return;
            }

            await _userRepository.UpdateUserAsync(user);

            _logger.LogInformation("User role changed successfully: {UserId} to RoleId: {RoleId}", user.Id, user.RoleId);
        }

        public async Task<ResponseDTO> ChangePassword(ChangePasswordRequest request)
        {
            if (request.NewPassword != request.ConfirmPassword)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Mật khẩu mới và xác nhận mật khẩu không khớp"
                };
            }

            var user = await _userRepository.GetUserByIdAsync(request.UserId);
            if (user == null)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Người dùng không tồn tại"
                };
            }

            var oldPasswordValid = BCrypt.Net.BCrypt.Verify(request.OldPassword, user.Password);
            if (!oldPasswordValid)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Mật khẩu không đúng"
                };
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _userRepository.UpdateUserAsync(user);

            return new ResponseDTO
            {
                IsSuccess = true,
                Message = "Đổi mật khẩu thành công"
            };
        }

        public async Task<ResponseDTO> ResetPasswordAsync(ResetPasswordRequest request)
        {
            try
            {
                // Validate
                if (string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Mật khẩu không được để trống"
                    };
                }

                if (request.NewPassword != request.ConfirmPassword)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Mật khẩu xác nhận không khớp"
                    };
                }

                if (request.NewPassword.Length < 6)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Mật khẩu phải có ít nhất 6 ký tự"
                    };
                }

                // ✅ Verify OTP lần nữa để đảm bảo security
                var otpResult = await _otpService.VerifyPasswordResetOtpAsync(request.Email, request.Otp);
                if (!otpResult.Success)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Mã OTP không hợp lệ hoặc đã hết hạn"
                    };
                }

                // Get user từ database
                var user = await _userRepository.GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Không tìm thấy người dùng"
                    };
                }

                // ✅ Hash password mới
                user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

                // ✅ Update vào database
                await _userRepository.UpdateUserAsync(user);

                _logger.LogInformation($"Password reset successfully for user: {user.Email}");

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Mật khẩu đã được đặt lại thành công. Bạn có thể đăng nhập với mật khẩu mới."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error resetting password for {request.Email}");
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra. Vui lòng thử lại sau."
                };
            }
        }
    }
}