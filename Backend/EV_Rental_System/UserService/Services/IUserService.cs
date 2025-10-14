using UserService.DTOs;
using UserService.Models;

namespace UserService.Services
{
    public interface IUserService
    {
        Task<List<StaffDTO?>> GetAllStaffAccount();
        Task<User> GetUserAsync(string userName);
        Task<LoginResponse> Login(LoginRequest loginRequest);
        Task<List<User>?> SearchUserAsync(string searchValue);
        Task<User> CreateUserFromRegistrationAsync(RegisterRequestDTO registerData);  // Create, tạo khi verify otp cho email thành công
        Task UpdateUserAsync(User user); // Edit, Update
        Task DeleteUserAsync(int userId); // Delete
        Task SetStatus(int userId); // Deactive/Active user
        Task<User?> GetUserByIdAsync(int userId); // Read
        Task<UserDetailDTO> GetUserDetailByIdAsync(int userId);
        Task<List<User?>> GetAllUsersAsync();
        Task SetAdmin(int userId);
        Task AddStaffAsync(StaffDTO staff); // Create staff user
        Task<ResponseDTO> ChangePassword(ChangePasswordRequest request);
        Task<ResponseDTO> ResetPasswordAsync(ResetPasswordRequest request);
    }
}
