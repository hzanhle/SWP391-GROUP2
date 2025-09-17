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

        public async Task AddCitizenInfo(CitizenInfo citizenInfo)
        {
            await _citizenInfoRepository.AddCitizenInfo(citizenInfo);
        }

        public async Task<CitizenInfo> GetCitizenInfoByUserId(int userId)
        {
            return await _citizenInfoRepository.GetCitizenInfoByUserId(userId);
        }

        public async Task UpdateCitizenInfo(CitizenInfo citizenInfo)
        {
            await _citizenInfoRepository.UpdateCitizenInfo(citizenInfo);
        }
    }
}
