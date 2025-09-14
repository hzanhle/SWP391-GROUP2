using UserService.Models;

namespace UserService.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserAsync(string userName, string password);
        Task<List<User>> SearchUserAsync(string searchValue);
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task<User> GetUserByIdAsync(int userId);
        Task<User?> GetUserDetailByIdAsync(int userId);
        Task<List<User>> GetAllUsersAsync();
        Task<CitizenInfo> GetCitizenInfoByUserId(int id);
        Task<DriverLicense> GetDriverLicenseByUserId(int id);
        Task AddDriverLicense(DriverLicense driverLicense);
        Task UpdateDriverLicense(DriverLicense driverLicense);
        Task AddCitizenInfo(CitizenInfo citizenInfo);
        Task UpdateCitizenInfo(CitizenInfo citizenInfo);
    }
}
