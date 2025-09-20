using UserService.Models;

namespace UserService.Services
{
    public interface IRoleService
    {
        Task<Role> GetRoleNameByIdAsync(int roleId);
        Task<Role> GetRoleByNameAsync(string roleName);
        Task<List<Role>> GetAllRole();
    }
}
