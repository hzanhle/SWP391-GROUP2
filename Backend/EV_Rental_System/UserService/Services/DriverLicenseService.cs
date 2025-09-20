using UserService.DTOs;
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
        public async Task AddDriverLicense(DriverLicenseRequest driverLicense)
        {
            DriverLicense info = new DriverLicense
            {
                UserId = driverLicense.UserId,
                LicenseId = driverLicense.LicenseId,
                LicenseType = driverLicense.LicenseType,
                RegisterDate = driverLicense.RegisterDate,
                ImageUrls = driverLicense.ImageUrls,
                RegisterOffice = driverLicense.RegisterOffice
            };
            await _driverLicenseRepository.AddDriverLicense(info);
        }

        public async Task<DriverLicense> GetDriverLicenseByUserId(int userId) 
            => await _driverLicenseRepository.GetDriverLicenseByUserId(userId);

        public async Task UpdateDriverLicense(DriverLicenseRequest driverLicense)
        {
            var existingLicense = await _driverLicenseRepository.GetDriverLicenseByUserId(driverLicense.UserId);

            existingLicense.LicenseId = driverLicense.LicenseId;
            existingLicense.LicenseType = driverLicense.LicenseType;
            existingLicense.RegisterDate = driverLicense.RegisterDate;
            existingLicense.RegisterOffice = driverLicense.RegisterOffice;
            
            await _driverLicenseRepository.UpdateDriverLicense(existingLicense);
        }
    }
}
