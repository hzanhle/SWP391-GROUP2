using UserService.DTOs;
using UserService.Models;

namespace UserService.Services
{
    public interface IUserService
    {
        Task<User> GetUserAsync(string userName);
        Task<LoginResponse> Login(LoginRequest loginRequest);
        Task<List<User>> SearchUserAsync(string searchValue);
        Task AddUserAsync(User user);  // Create
        Task UpdateUserAsync(User user); // Edit, Update
        Task DeleteUserAsync(int userId); // Delete (soft delete)
        Task<User?> GetUserByIdAsync(int userId); // Read
        Task<User?> GetUserDetailByIdAsync(int userId);
        Task<List<User>> GetAllUsersAsync();
        Task SetRole(int userId);
    }
}
