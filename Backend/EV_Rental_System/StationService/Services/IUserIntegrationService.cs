namespace StationService.Services
{
    public interface IUserIntegrationService
    {
        Task<string?> GetUserNameAsync(int userId);
        Task<Dictionary<int, string>> GetUserNamesBatchAsync(List<int> userIds);
    }
}
