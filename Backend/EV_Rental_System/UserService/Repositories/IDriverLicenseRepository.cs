using Microsoft.EntityFrameworkCore.Storage;
using UserService.Models;

namespace UserService.Repositories
{
    public interface IDriverLicenseRepository
    {
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<DriverLicense> GetDriverLicenseByUserId(int userId);
        Task AddDriverLicense(DriverLicense driverLicense);
        Task UpdateDriverLicense(DriverLicense driverLicense);
        Task DeleteDriverLicense(int userId);
        Task<DriverLicense> GetPendingDriverLicense(int userId);
        Task DeleteOldApprovedRecords(int userId, int keepId);
        Task ProcessDuplicateDriverLicense(int userId);
    }
}
