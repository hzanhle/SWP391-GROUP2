using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using UserService.Models;
using UserService.Models.Enums;

namespace UserService.Repositories
{
    public class CitizenInfoRepository : ICitizenInfoRepository
    {
        private readonly MyDbContext _context;

        public CitizenInfoRepository(MyDbContext context)
        {
            _context = context;
        }

        // ======================= TRANSACTION =======================

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        // ======================= CRUD =======================

        public async Task AddCitizenInfo(CitizenInfo citizenInfo)
        {
            await _context.CitizenInfos.AddAsync(citizenInfo);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCitizenInfo(CitizenInfo citizenInfo)
        {
            _context.CitizenInfos.Update(citizenInfo);
            await _context.SaveChangesAsync();
        }

        public async Task<CitizenInfo?> GetCitizenInfoByUserId(int userId)
        {
            return await _context.CitizenInfos
                .Include(ci => ci.Images)
                .FirstOrDefaultAsync(ci => ci.UserId == userId);
        }

        public async Task<CitizenInfo?> GetPendingCitizenInfo(int userId)
        {
            return await _context.CitizenInfos
                .Include(ci => ci.Images)
                .Where(ci => ci.UserId == userId && ci.Status == StatusInformation.Pending)
                .OrderByDescending(ci => ci.DayCreated)
                .FirstOrDefaultAsync();
        }

        public async Task DeleteOldApprovedRecords(int userId, int keepId)
        {
            var approvedRecords = await _context.CitizenInfos
                .Include(ci => ci.Images)
                .Where(ci => ci.UserId == userId && ci.Status == StatusInformation.Approved && ci.Id != keepId)
                .OrderBy(ci => ci.DayCreated)
                .ToListAsync();

            if (approvedRecords.Any())
            {
                var oldestRecord = approvedRecords.First();
                await DeleteCitizenInfo(oldestRecord.Id);
            }
        }

        public async Task DeleteCitizenInfo(int id)
        {
            var citizenInfo = await _context.CitizenInfos
                .FirstOrDefaultAsync(ci => ci.Id == id);

            if (citizenInfo != null)
            {
                _context.CitizenInfos.Remove(citizenInfo);
                await _context.SaveChangesAsync();
            }
        }
    }

    
}
