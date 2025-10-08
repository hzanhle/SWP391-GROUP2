using UserService.Models;

namespace UserService.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserAsync(string userName); // Lấy user theo userName
        Task<List<User>> SearchUserAsync(string searchValue); // Tìm kiếm user theo userName, email, phoneNumber
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task<User> GetUserByIdAsync(int userId); // Lấy user theo Id
        Task<User?> GetUserDetailByIdAsync(int userId); // Lấy user theo Id, bao gồm cả CitizenInfo và DriverLicense
        Task<List<User>> GetAllUsersAsync(); // Lấy tất cả user (cho admin)
        Task<User> GetUserByEmailAsync(string email);
    }
}
