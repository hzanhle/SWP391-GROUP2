using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.DTOs;
using UserService.Models;
using UserService.Repositories;

namespace UserService.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICitizenInfoRepository _citizenInfoRepository;
        private readonly ILogger<UserService> _logger;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;

        // Fixed constructor - inject all dependencies
        public UserService(
            IUserRepository userRepository,
            ICitizenInfoRepository citizenInfoRepository,
            ILogger<UserService> logger,
            IJwtService jwtService,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _citizenInfoRepository = citizenInfoRepository;
            _logger = logger;
            _jwtService = jwtService;
            _configuration = configuration;
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
                        RoleName = user.Role?.RoleName,
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

        // Removed duplicate GenerateJwtToken method since using IJwtService

        public async Task AddUserAsync(User user)
        {
            try
            {
                // Hash password before saving
                if (!string.IsNullOrEmpty(user.Password))
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                }

                user.CreatedAt = DateTime.UtcNow;
                user.IsActive = true; // Set default active status

                await _userRepository.AddUserAsync(user);
                _logger.LogInformation("User added successfully: {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user: {UserName}", user.UserName);
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
                    user.IsActive = false;
                    await _userRepository.UpdateUserAsync(user);
                    _logger.LogInformation("User deactivated: {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("Attempted to delete non-existent user: {UserId}", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", userId);
                throw;
            }
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            try
            {
                return await _userRepository.GetAllUsersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                throw;
            }
        }

        
        public async Task<User?> GetUserByUsernameAsync(string userName)
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

                UpdateUserRequest userRequest = new UpdateUserRequest
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,                   
                };

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

        // Additional helper methods
        public async Task<bool> IsUserExistAsync(string userName, string email)
        {
            try
            {
                var userByUsername = await _userRepository.GetUserByUsernameAsync(userName);
                var userByEmail = await _userRepository.GetUserByEmailAsync(email);

                return userByUsername != null || userByEmail != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user exists");
                throw;
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                    return false;

                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.Password))
                    return false;

                // Update with new password
                user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateUserAsync(user);
                _logger.LogInformation("Password changed successfully for user: {UserId}", userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
                throw;
            }
        }
    }
}