using UserService.Models;
using UserService.Models.UserService.Models;

namespace UserService.Repositories
{
    public interface IDriverLicenseRepository
    {
        Task<DriverLicense> GetDriverLicenseByUserId(int userId);
        Task AddDriverLicense(DriverLicense driverLicense);
        Task UpdateDriverLicense(DriverLicense driverLicense);
        Task DeleteDriverLicense(int userId);
    }
}
