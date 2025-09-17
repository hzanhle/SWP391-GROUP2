using UserService.Models;

namespace UserService.Services
{
    public interface ICitizenInfoService
    {
        Task UpdateCitizenInfo(CitizenInfo citizenInfo);
        Task AddCitizenInfo(CitizenInfo citizenInfo);
        Task<CitizenInfo> GetCitizenInfoByUserId(int userId);
    }
}
