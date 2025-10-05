using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly MyDbContext _context;
        public RoleRepository(MyDbContext context)
        {
            _context = context;
        }
        public async Task<Role> GetRoleByNameAsync(string roleName)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == roleName);
        }

        public async Task<Role> GetRoleByIdAsync(int roleId)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleId == roleId);
        }

        public async Task<List<Role>> GetAllRole()
        {
            return await _context.Roles.ToListAsync();
        }
    }
}
