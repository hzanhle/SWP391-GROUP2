using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Repositories
{
    public class DriverLicenseRepository : IDriverLicenseRepository
    {
        private readonly MyDbContext _context;

        public DriverLicenseRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task AddDriverLicense(DriverLicense driverLicense)
        {
            await _context.DriverLicenses.AddAsync(driverLicense);
            await _context.SaveChangesAsync();
        }


        public async Task UpdateDriverLicense(DriverLicense driverLicense)
        {
            _context.DriverLicenses.Update(driverLicense);
            await _context.SaveChangesAsync();
        }



        public async Task<DriverLicense> GetDriverLicenseByUserId(int userId)
        {
            return await _context.DriverLicenses.FirstOrDefaultAsync(dl => dl.UserId == userId);
        }

        public Task DeleteDriverLicense(int userId)
        {
            throw new NotImplementedException();
        }
    }
}
