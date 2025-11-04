using UserService.DTOs;
using UserService.Models;

namespace UserService.Services
{
    public interface ICitizenInfoService
    {
        Task<ResponseDTO> AddCitizenInfo(CitizenInfoRequest request, int userId);
        Task<CitizenInfoDTO> GetCitizenInfoByUserId(int userId);
        Task<Notification> SetStatus(int userId, bool isApproved); // Đổi từ Task<string>
        Task UpdateCitizenInfo(CitizenInfoRequest request, int userId);

        Task DeleteCitizenInfo(int userId);
    }
}