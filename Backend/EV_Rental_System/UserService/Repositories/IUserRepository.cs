using UserService.DTOs;
using UserService.Models;

namespace UserService.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserAsync(string userName); // Lấy user theo userName
        Task<List<User>> SearchUserAsync(string searchValue); // Tìm kiếm user theo userName, email, phoneNumber
        Task<List<User>?> GetAllStaffAccount(); //Lấy toàn bộ tài khoản Staff
        Task AddUserAsync(User user);
        Task DeleteUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task<User> GetUserByIdAsync(int userId); // Lấy user theo Id
        Task<User?> GetUserDetailByIdAsync(int userId); // Lấy user theo Id, bao gồm cả CitizenInfo và DriverLicense
        Task<List<User>> GetAllUsersAsync(); // Lấy tất cả user (cho admin)
        Task<User> GetUserByEmailAsync(string email);
    }
}
