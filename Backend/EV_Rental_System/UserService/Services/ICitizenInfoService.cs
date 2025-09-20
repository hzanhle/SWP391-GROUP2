using UserService.DTOs;
using UserService.Models;

namespace UserService.Services
{
    public interface ICitizenInfoService
    {
        Task UpdateCitizenInfo(CitizenInfoRequest citizenInfo);
        Task AddCitizenInfo(CitizenInfoRequest citizenInfo);
        Task<CitizenInfo> GetCitizenInfoByUserId(int userId);
    }
}
