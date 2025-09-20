using UserService.DTOs;
using UserService.Models;
using UserService.Repositories;

namespace UserService.Services
{
    public class CitizenInfoService : ICitizenInfoService
    {
        private readonly ICitizenInfoRepository _citizenInfoRepository;

        public CitizenInfoService(ICitizenInfoRepository citizenInfoRepository)
        {
           _citizenInfoRepository = citizenInfoRepository;
        }

        public async Task AddCitizenInfo(CitizenInfoRequest citizenInfo)
        {
            CitizenInfo info = new CitizenInfo
            {
                Address = citizenInfo.Address,
                DayOfBirth = citizenInfo.DayOfBirth,
                FullName = citizenInfo.FullName,
                ImageUrls = citizenInfo.ImageUrls,
                UserId = citizenInfo.UserId,
                CitizenId = citizenInfo.CitizenId

            };
            await _citizenInfoRepository.AddCitizenInfo(info);
        }

        public async Task<CitizenInfo> GetCitizenInfoByUserId(int userId)
        {
            return await _citizenInfoRepository.GetCitizenInfoByUserId(userId);
        }

        public async Task UpdateCitizenInfo(CitizenInfoRequest citizenInfo)
        {
            var existingInfo = await _citizenInfoRepository.GetCitizenInfoByUserId(citizenInfo.UserId);
            existingInfo.Address = citizenInfo.Address;
            existingInfo.DayOfBirth = citizenInfo.DayOfBirth;
            existingInfo.FullName = citizenInfo.FullName;
            existingInfo.ImageUrls = citizenInfo.ImageUrls;
            existingInfo.CitizenId = citizenInfo.CitizenId;

            await _citizenInfoRepository.UpdateCitizenInfo(existingInfo);
        }
    }
}
