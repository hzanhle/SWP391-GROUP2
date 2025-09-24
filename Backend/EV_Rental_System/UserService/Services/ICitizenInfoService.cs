using UserService.DTOs;
using UserService.Models;
using UserService.Models.UserService.Models;

namespace UserService.Services
{
    public interface ICitizenInfoService
    {
        Task AddCitizenInfo(CitizenInfoRequest request);
        Task<CitizenInfo> GetCitizenInfoByUserId(int userId);
        Task UpdateCitizenInfo(CitizenInfoRequest request);
    }
}