using UserService.DTOs;
using UserService.Models;
using UserService.Models.UserService.Models;

namespace UserService.Services
{
    public interface IDriverLicenseService
    {
        Task<ResponseDTO> AddDriverLicense(DriverLicenseRequest request, int userId);
        Task<DriverLicenseDTO> GetDriverLicenseByUserId(int userId);
        Task<Notification> SetStatus(int userId, bool isApproved);
        Task UpdateDriverLicense(DriverLicenseRequest request, int userId);
        Task DeleteDriverLicense(int Id);
    }
}
