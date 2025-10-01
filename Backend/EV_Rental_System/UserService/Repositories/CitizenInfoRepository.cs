using Microsoft.EntityFrameworkCore;
using UserService.Models;
using UserService.Models.UserService.Models;

namespace UserService.Repositories
{
    public class CitizenInfoRepository : ICitizenInfoRepository
    {
        private readonly MyDbContext _context;

        public CitizenInfoRepository(MyDbContext context)
        {
            _context = context;
        }

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

        public async Task<CitizenInfo> GetCitizenInfoByUserId(int userId)
        {
            return await _context.CitizenInfos
                .Include(ci => ci.Images)
                .FirstOrDefaultAsync(ci => ci.UserId == userId);
        }

        
        /// Lấy bản CitizenInfo đang chờ xác thực (pending)
        public async Task<CitizenInfo> GetPendingCitizenInfo(int userId)
        {
            return await _context.CitizenInfos
                .Include(ci => ci.Images)
                .Where(ci => ci.UserId == userId && ci.Status == "Chờ xác thực")
                .OrderByDescending(ci => ci.DayCreated)
                .FirstOrDefaultAsync();
        }


        /// Xóa tất cả các bản đã xác nhận cũ, giữ lại bản mới được approve
        public async Task DeleteOldApprovedRecords(int userId, int keepId)
        {
            // Lấy tất cả bản đã xác nhận của user, ngoại trừ bản mới được xác thực
            var approvedRecords = await _context.CitizenInfos
                .Include(ci => ci.Images)
                .Where(ci => ci.UserId == userId && ci.Status == "Đã xác nhận" && ci.Id != keepId)
                .OrderBy(ci => ci.DayCreated) // cũ nhất trước
                .ToListAsync();

            if (approvedRecords.Any())
            {
                // Chỉ xóa bản cũ nhất
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