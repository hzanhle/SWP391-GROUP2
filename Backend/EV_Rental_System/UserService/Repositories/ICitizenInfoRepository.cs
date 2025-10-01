using UserService.Models;
using UserService.Models.UserService.Models;

namespace UserService.Repositories
{
    public interface ICitizenInfoRepository
    {
        Task AddCitizenInfo(CitizenInfo citizenInfo);
        Task UpdateCitizenInfo(CitizenInfo citizenInfo);
        Task<CitizenInfo> GetCitizenInfoByUserId(int userId);
        Task DeleteCitizenInfo(int userId);
        Task<CitizenInfo> GetPendingCitizenInfo(int userId);
        Task DeleteOldApprovedRecords(int userId, int keepId);


    }
}