using UserService.DTOs;
using UserService.Models;
using UserService.Models.UserService.Models;

namespace UserService.Services
{
    public interface ICitizenInfoService
    {
        Task<CitizenInfo> AddCitizenInfo(CitizenInfoRequest request);
        Task<CitizenInfoDTO> GetCitizenInfoByUserId(int userId);
        Task SetStatus(int userId);
        Task UpdateCitizenInfo(CitizenInfoRequest request);
    }
}