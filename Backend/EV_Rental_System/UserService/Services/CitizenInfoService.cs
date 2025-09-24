using UserService.DTOs;
using UserService.Models;
using UserService.Models.UserService.Models;
using UserService.Repositories;

namespace UserService.Services
{
    public class CitizenInfoService : ICitizenInfoService
    {
        private readonly ICitizenInfoRepository _citizenInfoRepository;
        private readonly IImageService _imageService;

        public CitizenInfoService(ICitizenInfoRepository citizenInfoRepository, IImageService imageService)
        {
            _citizenInfoRepository = citizenInfoRepository;
            _imageService = imageService;
        }

        public async Task AddCitizenInfo(CitizenInfoRequest request)
        {
            // 1. Tạo entity CitizenInfo
            var entity = new CitizenInfo
            {
                Address = request.Address,
                DayOfBirth = request.DayOfBirth,
                FullName = request.FullName,
                UserId = request.UserId,
                CitizenId = request.CitizenId
            };

            // 2. Lưu CitizenInfo để EF Core gán Id
            await _citizenInfoRepository.AddCitizenInfo(entity);

            // 3. Upload file và lưu Image nếu FE gửi file nhị phân
            if (request.Files != null && request.Files.Count > 0)
            {
                var images = await _imageService.UploadImagesAsync(request.Files, "CitizenInfo", entity.Id);

                // Không cần gán entity.Images vì EF Core tự handle navigation property
                // Chỉ cần lưu từng image vào DB
                foreach (var img in images)
                {
                    await _imageService.AddImage(img);
                }
            }
        }

        public async Task<CitizenInfo> GetCitizenInfoByUserId(int userId)
        {
            return await _citizenInfoRepository.GetCitizenInfoByUserId(userId);
        }

        public async Task SetStatus(int userId)
        {
            var entity = await _citizenInfoRepository.GetCitizenInfoByUserId(userId);
            if (entity == null)
                throw new Exception("CitizenInfo not found");
            entity.Status = "Đã xác nhận";
            await _citizenInfoRepository.UpdateCitizenInfo(entity);
        }

        public async Task UpdateCitizenInfo(CitizenInfoRequest request)
        {
            var entity = await _citizenInfoRepository.GetCitizenInfoByUserId(request.UserId);
            if (entity == null)
                throw new Exception("CitizenInfo not found");

            // Cập nhật thông tin cơ bản
            entity.Address = request.Address;
            entity.DayOfBirth = request.DayOfBirth;
            entity.FullName = request.FullName;
            entity.CitizenId = request.CitizenId;

            // Xử lý hình ảnh: xóa cũ và upload mới
            await _imageService.DeleteImagesAsync("CitizenInfo", entity.Id);

            if (request.Files != null && request.Files.Count > 0)
            {
                var images = await _imageService.UploadImagesAsync(request.Files, "CitizenInfo", entity.Id);

                // Không cần gán entity.Images
                foreach (var img in images)
                {
                    await _imageService.AddImage(img);
                }
            }

            await _citizenInfoRepository.UpdateCitizenInfo(entity);
        }
    }
}