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

        public async Task DeleteCitizenInfo(int userId)
        {
            var citizenInfo = await _context.CitizenInfos
                .Include(ci => ci.Images)
                .FirstOrDefaultAsync(ci => ci.UserId == userId);

            if (citizenInfo != null)
            {
                _context.CitizenInfos.Remove(citizenInfo);
                await _context.SaveChangesAsync();
            }
        }
    }
}