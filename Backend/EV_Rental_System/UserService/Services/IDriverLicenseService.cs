using UserService.DTOs;
using UserService.Models;

namespace UserService.Services
{
    public interface IDriverLicenseService
    {
        Task UpdateDriverLicense(DriverLicenseRequest driverLicense);
        Task AddDriverLicense(DriverLicenseRequest driverLicense);
        Task<DriverLicense> GetDriverLicenseByUserId(int userId);
    }
}
