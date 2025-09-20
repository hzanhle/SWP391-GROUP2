using UserService.Models;
using UserService.Repositories;

namespace UserService.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }
        public async Task<Role> GetRoleByNameAsync(string roleName)
        {
            return await _roleRepository.GetRoleByNameAsync(roleName);
        }
        public async Task<Role> GetRoleNameByIdAsync(int roleId)
        {
            return await _roleRepository.GetRoleByIdAsync(roleId);
        }
        public async Task<List<Role>> GetAllRole()
        {
            return await _roleRepository.GetAllRole();
        }
    }
}
