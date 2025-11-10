
using System.Text.Json;

namespace StationService.Services
{
    public class UserIntegrationService : IUserIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserIntegrationService> _logger;
        private readonly string _userServiceUrl;

        public UserIntegrationService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<UserIntegrationService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            // Lấy URL từ appsettings.json
            _userServiceUrl = configuration["ServiceUrls:UserService"]
                ?? "http://localhost:5000"; // Default
        }
        
        /// Lấy tên user từ UserService
        public async Task<string?> GetUserNameByIdAsync(int userId)
        {
            try
            {
                var userInfo = await GetUserInfoByIdAsync(userId);
                return userInfo?.UserName;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to get username for user {userId}");
                return null; // Fallback gracefully
            }
        }

        /// Lấy thông tin user chi tiết
        public async Task<UserInfoDTO?> GetUserInfoByIdAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{_userServiceUrl}/api/users/{userId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        $"UserService returned {response.StatusCode} for user {userId}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();

                // Phân tích phản hồi
                var userResponse = JsonSerializer.Deserialize<UserServiceResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (userResponse?.Data == null)
                {
                    return null;
                }

                return new UserInfoDTO
                {
                    UserId = userResponse.Data.UserId,
                    UserName = userResponse.Data.UserName,
                    Email = userResponse.Data.Email
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching user info for user {userId}");
                return null; // Fail gracefully
            }
        }

        /// Lấy nhiều users cùng lúc
        public async Task<Dictionary<int, string>> GetUserNamesByIdsAsync(
            IEnumerable<int> userIds)
        {
            var result = new Dictionary<int, string>();

            if (!userIds.Any())
            {
                return result;
            }

            try
            {
                var tasks = userIds.Select(async userId =>
                {
                    var userName = await GetUserNameByIdAsync(userId);
                    return new { UserId = userId, UserName = userName };
                });

                var users = await Task.WhenAll(tasks);

                foreach (var user in users)
                {
                    if (user.UserName != null)
                    {
                        result[user.UserId] = user.UserName;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching batch user names");
                return result;
            }
        }

        // ==================== HELPER CLASSES ====================

        private class UserServiceResponse
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public UserData? Data { get; set; }
        }

        private class UserData
        {
            public int UserId { get; set; }
            public string UserName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? RoleName { get; set; }
        }
    }
}