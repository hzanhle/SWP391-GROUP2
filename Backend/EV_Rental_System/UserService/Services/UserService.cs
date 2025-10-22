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
        private readonly ICitizenInfoService _citizenInfoService;
        private readonly IDriverLicenseService _driverLicenseService;
        private readonly ILogger<UserService> _logger;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;
        private readonly IOtpService _otpService;


        public UserService(
            IRoleRepository roleRepository,
            IUserRepository userRepository,
            ICitizenInfoService citizenInfoService,
            IDriverLicenseService driverLicenseService,
            ILogger<UserService> logger,
            IJwtService jwtService,
            IConfiguration configuration,
            IOtpService otpService)
        {
            _userRepository = userRepository;
            _citizenInfoService = citizenInfoService;
            _driverLicenseService = driverLicenseService;
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
                var citizenInfo = await _citizenInfoService.GetCitizenInfoByUserId(user.Id);

                return new LoginResponse
                {
                    IsSuccess = true,
                    Message = "Đăng nhập thành công",
                    Token = token,
                    TokenType = "Bearer",
                    ExpiresIn = _configuration.GetSection("JwtSettings").GetValue<int>("ExpiresInMinutes") * 60,
                    User = new StaffDTO
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
        public async Task<User> CreateUserFromRegistrationAsync(RegisterRequestDTO registerData)
        {
            try
            {
                // Kiểm tra lại username (double check)
                var existingUser = await _userRepository.GetUserAsync(registerData.UserName);
                if (existingUser != null)
                {
                    _logger.LogWarning("Attempted to create user with existing username: {UserName}", registerData.UserName);
                    throw new ArgumentException("Tên đăng nhập đã tồn tại");
                }

                // Kiểm tra lại email (double check)
                var existingEmail = await _userRepository.GetUserByEmailAsync(registerData.Email);
                if (existingEmail != null)
                {
                    _logger.LogWarning("Attempted to create user with existing email: {Email}", registerData.Email);
                    throw new ArgumentException("Email đã được sử dụng");
                }

                // Lấy role mặc định
                var role = await _roleRepository.GetRoleByNameAsync("Member");
                if (role == null)
                {
                    _logger.LogError("Role 'Member' not found in database");
                    throw new InvalidOperationException("Không tìm thấy role mặc định");
                }

                // Tạo user mới
                var user = new User
                {
                    UserName = registerData.UserName,
                    Email = registerData.Email,
                    PhoneNumber = registerData.PhoneNumber,
                    Password = BCrypt.Net.BCrypt.HashPassword(registerData.Password), // Hash password
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    RoleId = role.RoleId,
                    Role = role
                };

                await _userRepository.AddUserAsync(user);

                _logger.LogInformation("User registered successfully: {UserId}, Username: {UserName}",
                    user.Id, user.UserName);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user from registration: {UserName}", registerData.UserName);
                throw;
            }
        }
        public async Task AddStaffAsync(StaffDTO staffRequest)
        {
            try
            {
                // Kiểm tra username đã tồn tại
                var existingUser = await _userRepository.GetUserAsync(staffRequest.UserName);
                if (existingUser != null)
                {
                    _logger.LogWarning("Attempted to add staff with existing username: {UserName}", staffRequest.UserName);
                    throw new ArgumentException("Tên đăng nhập đã tồn tại");
                }

                // Lấy role Employee mặc định cho staff
                var role = await _roleRepository.GetRoleByNameAsync("Employee");
                if (role == null)
                {
                    _logger.LogWarning("Employee role not found");
                    throw new InvalidOperationException("Vai trò Employee không tồn tại trong hệ thống");
                }

                // Tạo đối tượng User mới
                var staff = new User
                {
                    UserName = staffRequest.UserName,
                    FullName = staffRequest.FullName,
                    Email = staffRequest.Email,
                    PhoneNumber = staffRequest.PhoneNumber,
                    StationId = staffRequest.StationId,
                    Password = BCrypt.Net.BCrypt.HashPassword(staffRequest.Password),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    RoleId = role.RoleId,
                    Role = role
                };

                // Thêm user vào database
                await _userRepository.AddUserAsync(staff);

                _logger.LogInformation("Staff user added successfully: {UserId} - {UserName}", staff.Id, staff.UserName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding staff user: {UserName}", staffRequest.UserName);
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
                    user.IsActive = !user.IsActive;
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
                    await _userRepository.DeleteUserAsync(user);
                }
            }
            catch (Exception ex)
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
        public async Task<UserDetailDTO> GetUserDetailByIdAsync(int userId)
        {
            try
            {
                // Query User TRƯỚC
                var user = await _userRepository.GetUserDetailByIdAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning("User not found with ID: {UserId}", userId);
                    return null;
                }

                // Sau đó mới query các thông tin liên quan
                var citizenInfo = await _citizenInfoService.GetCitizenInfoByUserId(userId);
                var driverLicense = await _driverLicenseService.GetDriverLicenseByUserId(userId);

                var dto = new UserDetailDTO
                {
                    UserId = user.Id,
                    PhoneNumber = user.PhoneNumber ?? string.Empty, // Thêm null check
                    Email = user.Email ?? string.Empty,
                    UserName = user.UserName ?? string.Empty,
                    CitizenInfo = citizenInfo,
                    DriverLicense = driverLicense
                };

                return dto;
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
                if (!otpResult.Success == true)
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
        public async Task<List<StaffDTO?>> GetAllStaffAccount()
        {
            try
            {
                var dtoList = new List<StaffDTO>();
                var staffList = await _userRepository.GetAllStaffAccount();
                foreach (var staff in staffList)
                {
                    var dto = new StaffDTO
                    {
                        Email = staff.Email,
                        PhoneNumber = staff.PhoneNumber,
                        Password = staff.Password,
                        RoleId = staff.RoleId,
                        RoleName = staff.Role.RoleName,
                        StationId = staff.StationId,
                        FullName = staff.FullName,
                        Id = staff.Id,
                        UserName = staff.UserName,
                        IsActive = staff.IsActive
                    };
                    dtoList.Add(dto);
                }
                return dtoList;
            } catch (Exception ex)
            {
                throw new Exception();
            }
        }
    }
}


