using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly MyDbContext _context;

        public UserRepository(MyDbContext context)
        {
            _context = context;
        }
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
               .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User?> GetUserDetailByIdAsync(int userId)
        {
            return await _context.Users
                    .Include(u => u.CitizenInfo)
                    .Include(u => u.DriverLicense)
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User> GetUserAsync(string userName)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
        }

        public async Task<List<User>> SearchUserAsync(string searchValue)
        {
            return await _context.Users
                .Where(u => u.UserName.Contains(searchValue) || u.Email.Contains(searchValue) || u.PhoneNumber.Contains(searchValue))
                .ToListAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Update(user);
            await _context.SaveChangesAsync();
        }

    }
}
