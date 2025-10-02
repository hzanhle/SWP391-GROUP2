using UserService.DTOs;
using UserService.Models;
using UserService.Models.UserService.Models;

namespace UserService.Services
{
    public interface ICitizenInfoService
    {
        Task<ResponseDTO> AddCitizenInfo(CitizenInfoRequest request);
        Task<CitizenInfoDTO> GetCitizenInfoByUserId(int userId);
        Task<Notification> SetStatus(int userId, bool isApproved); // Đổi từ Task<string>
        Task UpdateCitizenInfo(CitizenInfoRequest request);

        Task DeleteCitizenInfo(int userId);
    }
}