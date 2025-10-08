using Microsoft.EntityFrameworkCore;
using UserService.Models;
using UserService.Models.UserService.Models;

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
            return await _context.DriverLicenses
                .Include(dl => dl.Images)
                .FirstOrDefaultAsync(dl => dl.UserId == userId);
        }

        public async Task<DriverLicense> GetPendingDriverLicense(int userId)
        {
            return await _context.DriverLicenses
                .Include(dl => dl.Images)
                .Where(dl => dl.UserId == userId && dl.Status == "Chờ xác thực")
                .OrderByDescending(dl => dl.DateCreated)
                .FirstOrDefaultAsync();
        }

        public async Task DeleteOldApprovedRecords(int userId, int keepId)
        {
            var oldApprovedRecords = await _context.DriverLicenses
                .Include(dl => dl.Images)
                .Where(dl => dl.UserId == userId
                    && dl.Status == "Đã xác nhận"
                    && dl.Id != keepId)
                .ToListAsync();

            if (oldApprovedRecords.Any())
            {
                _context.DriverLicenses.RemoveRange(oldApprovedRecords);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteDriverLicense(int Id)
        {
            var driverLicense = await _context.DriverLicenses
                .FirstOrDefaultAsync(dl => dl.Id == Id);

            if (driverLicense != null)
            {
                _context.DriverLicenses.Remove(driverLicense);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ProcessDuplicateDriverLicense(int userId)
        {
            var driverLicenses = await _context.DriverLicenses
                .Where(dl => dl.UserId == userId)
                .ToListAsync();

            var duplicates = driverLicenses
                .GroupBy(dl => dl.LicenseId)
                .Where(g => g.Count() > 1);

            foreach (var group in duplicates)
            {
                var ordered = group.OrderByDescending(dl => dl.DateCreated).ToList();
                var toDelete = ordered.Skip(1).ToList();

                foreach (var oldRecord in toDelete)
                {
                    _context.DriverLicenses.Remove(oldRecord);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}