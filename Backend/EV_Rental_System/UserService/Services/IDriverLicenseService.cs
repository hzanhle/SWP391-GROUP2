using UserService.Models;

namespace UserService.Services
{
    public interface IDriverLicenseService
    {
        Task UpdateDriverLicense(DriverLicense driverLicense);
        Task AddDriverLicense(DriverLicense driverLicense);
        Task<DriverLicense> GetDriverLicenseByUserId(int userId);
    }
}
