using UserService.Models;
using UserService.Repositories;

namespace UserService.Services
{
    public class UserService : IUserService
    {
        private readonly UserRepository _userRepository;

        public UserService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task AddCitizenInfo(CitizenInfo citizenInfo)
        {
            await _userRepository.AddCitizenInfo(citizenInfo);
        }

        public async Task AddDriverLicense(DriverLicense driverLicense)
        {
            await _userRepository.AddDriverLicense(driverLicense);
        }

        public async Task AddUserAsync(User user)
        {
            await _userRepository.AddUserAsync(user);
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user != null)
            {
                user.IsActive = false;
                await _userRepository.UpdateUserAsync(user);
            }
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }

        public async Task<CitizenInfo> GetCitizenInfoByUserId(int userId)
        {
            return await _userRepository.GetCitizenInfoByUserId(userId);
        }

        public async Task<DriverLicense> GetDriverLicenseByUserId(int userId)
        {
            return await _userRepository.GetDriverLicenseByUserId(userId);
        }

        public async Task<User> GetUserAsync(string userName, string password)
        {
            return await _userRepository.GetUserAsync(userName, password);
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _userRepository.GetUserByIdAsync(userId);
        }

        public async Task<List<User>> SearchUserAsync(string searchValue)
        {
            return await _userRepository.GetAllUsersAsync();
        }

        public async Task UpdateCitizenInfo(CitizenInfo citizenInfo)
        {
            var existingInfo = await _userRepository.GetCitizenInfoByUserId(citizenInfo.UserId);
            if (existingInfo != null)
            {
                existingInfo.CitiRegisOffice = citizenInfo.CitiRegisOffice;
                existingInfo.CitiRegisDate = citizenInfo.CitiRegisDate;
                existingInfo.CitizenId = citizenInfo.CitizenId;
                existingInfo.Sex = citizenInfo.Sex;
                existingInfo.FullName = citizenInfo.FullName;
                existingInfo.Address = citizenInfo.Address;
                existingInfo.BirthDate = citizenInfo.BirthDate;
            }
            await _userRepository.UpdateCitizenInfo(citizenInfo);
        }

        public async Task UpdateDriverLicense(DriverLicense driverLicense)
        {
            await _userRepository.UpdateDriverLicense(driverLicense);
        }

        public async Task UpdateUserAsync(User user)
        {
            var existingUser = await _userRepository.GetUserByIdAsync(user.Id);
            if (existingUser != null)
            {
                existingUser.UserName = user.UserName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;
            }
            await _userRepository.UpdateUserAsync(user);
        }
    }
}
