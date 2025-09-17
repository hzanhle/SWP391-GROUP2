using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Repositories
{
    public class CitizenInfoRepository : ICitizenInfoRepository
    {
        private readonly MyDbContext _context;

        public CitizenInfoRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<CitizenInfo> GetCitizenInfoByUserId(int userId)
        {
            return await _context.CitizenInfos.FirstOrDefaultAsync(co => co.UserId == userId);
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

        public Task DeleteCitizenInfo(int id)
        {
            throw new NotImplementedException();
        }
    }
}
