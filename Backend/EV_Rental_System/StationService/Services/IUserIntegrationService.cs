namespace StationService.Services
{
    public interface IUserIntegrationService
    {
        Task<string?> GetUserNameByIdAsync(int userId); //Lấy tên user từ UserService theo userId
        Task<Dictionary<int, string>> GetUserNamesByIdsAsync(IEnumerable<int> userIds); //Lấy nhiều users cùng lúc
        Task<UserInfoDTO?> GetUserInfoByIdAsync(int userId); //Lấy thông tin user từ UserService theo userId
    }

    public class UserInfoDTO
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
