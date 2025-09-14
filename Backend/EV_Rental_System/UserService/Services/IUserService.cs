using UserService.Models;

namespace UserService.Services
{
    public interface IUserService
    {
        Task<User> GetUserAsync(string userName, string password);
        Task<List<User>> SearchUserAsync(string searchValue);
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int userId);
        Task<User?> GetUserByIdAsync(int userId);
        Task<List<User>> GetAllUsersAsync();
        Task AddDriverLicense(DriverLicense driverLicense);
        Task<DriverLicense> GetDriverLicenseByUserId(int userId);
        Task<CitizenInfo> GetCitizenInfoByUserId(int userId);
        Task UpdateDriverLicense(DriverLicense driverLicense);
        Task AddCitizenInfo(CitizenInfo citizenInfo);
        Task UpdateCitizenInfo(CitizenInfo citizenInfo);
    }
}
