using UserService.DTOs;
using UserService.Models;
using UserService.Models.UserService.Models;

namespace UserService.Services
{
    public interface IDriverLicenseService
    {
        Task<DriverLicense> AddDriverLicense(DriverLicenseRequest request);
        Task<DriverLicenseDTO> GetDriverLicenseByUserId(int userId);
        Task<Notification> SetStatus(int userId, bool isApproved);
        Task UpdateDriverLicense(DriverLicenseRequest request);
        Task DeleteDriverLicense(int Id);
    }
}
