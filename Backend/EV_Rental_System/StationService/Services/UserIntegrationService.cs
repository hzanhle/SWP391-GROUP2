using System.Text.Json;

namespace StationService.Services
{
    public class UserIntegrationService : IUserIntegrationService
    {
        private readonly IHttpClientFactory _httpClientFactory; //LỖI CHƯA LẤY TÊN NHÂN VIÊN - CẦN FIX  
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserIntegrationService> _logger;

        public UserIntegrationService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<UserIntegrationService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string?> GetUserNameAsync(int userId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("UserService");
                var response = await client.GetAsync($"/api/users/{userId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get user {UserId} from UserService. Status: {Status}",
                        userId, response.StatusCode);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var userResponse = JsonSerializer.Deserialize<UserApiResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return userResponse?.Data?.FullName ?? userResponse?.Data?.UserName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling UserService for userId {UserId}", userId);
                return null;
            }
        }

        public async Task<Dictionary<int, string>> GetUserNamesBatchAsync(List<int> userIds)
        {
            var result = new Dictionary<int, string>();

            // Call API for each user (trong production có thể tối ưu bằng batch API)
            var tasks = userIds.Distinct().Select(async userId =>
            {
                var name = await GetUserNameAsync(userId);
                return new { UserId = userId, Name = name };
            });

            var names = await Task.WhenAll(tasks);

            foreach (var item in names)
            {
                if (item.Name != null)
                {
                    result[item.UserId] = item.Name;
                }
            }

            return result;
        }

        private class UserApiResponse
        {
            public bool Success { get; set; }
            public UserData? Data { get; set; }
        }

        private class UserData
        {
            public int Id { get; set; }
            public string? UserName { get; set; }
            public string? FullName { get; set; }
            public string? Email { get; set; }
        }

    }
}
