using UserService.Models;

namespace UserService.Repositories
{
    public interface ICitizenInfoRepository
    {
        Task AddCitizenInfo(CitizenInfo citizenInfo);
        Task UpdateCitizenInfo(CitizenInfo citizenInfo);
        Task<CitizenInfo> GetCitizenInfoByUserId(int id);
        Task DeleteCitizenInfo(int id);
    }
}
