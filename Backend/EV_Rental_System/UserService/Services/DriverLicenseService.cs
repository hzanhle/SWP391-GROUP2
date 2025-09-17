using UserService.Models;
using UserService.Repositories;

namespace UserService.Services
{
    public class DriverLicenseService : IDriverLicenseService
    {
        private readonly IDriverLicenseRepository _driverLicenseRepository;

        public DriverLicenseService(IDriverLicenseRepository driverLicenseRepository)
        {
            _driverLicenseRepository = driverLicenseRepository;
        }
        public async Task AddDriverLicense(DriverLicense driverLicense)
        {
            await _driverLicenseRepository.AddDriverLicense(driverLicense);
        }

        public async Task<DriverLicense> GetDriverLicenseByUserId(int userId) 
            => await _driverLicenseRepository.GetDriverLicenseByUserId(userId);

        public async Task UpdateDriverLicense(DriverLicense driverLicense)
        {
            await _driverLicenseRepository.UpdateDriverLicense(driverLicense);
        }
    }
}
