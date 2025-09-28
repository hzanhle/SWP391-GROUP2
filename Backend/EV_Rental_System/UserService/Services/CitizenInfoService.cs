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

        public async Task<CitizenInfo> AddCitizenInfo(CitizenInfoRequest request)
        {
            // ✅ Debug: Check files trước khi xử lý
            Console.WriteLine($"🔍 AddCitizenInfo called for UserId: {request.UserId}");
            Console.WriteLine($"🔍 Files count: {request.Files?.Count ?? 0}");

            if (request.Files != null)
            {
                for (int i = 0; i < request.Files.Count; i++)
                {
                    var file = request.Files[i];
                    Console.WriteLine($"📎 File {i}: {file.FileName}, Size: {file.Length} bytes");
                }
            }

            // 1. Tạo entity CitizenInfo với đầy đủ thông tin
            var entity = new CitizenInfo
            {
                Address = request.Address,
                DayOfBirth = request.DayOfBirth,
                FullName = request.FullName,
                UserId = request.UserId,
                CitizenId = request.CitizenId,
                CitiRegisOffice = request.CitiRegisOffice,
                Sex = request.Sex,
                CitiRegisDate = request.CitiRegisDate,
                Status = "Chờ Xác Thực"
            };

            // 2. Lưu CitizenInfo để EF Core gán Id
            await _citizenInfoRepository.AddCitizenInfo(entity);
            Console.WriteLine($"✅ CitizenInfo saved with Id: {entity.Id}");

            // 3. Upload file và lưu Image nếu FE gửi file nhị phân
            if (request.Files != null && request.Files.Count > 0)
            {
                Console.WriteLine($"🚀 Starting upload for typeId: {entity.Id}");
                var images = await _imageService.UploadImagesAsync(request.Files, "CitizenInfo", entity.Id);
                Console.WriteLine($"🎯 Upload result: {images.Count} images uploaded");
            }
            else
            {
                Console.WriteLine("⚠️ No files to upload");
            }

            // 4. ✅ Load lại từ database với Images included
            return await _citizenInfoRepository.GetCitizenInfoByUserId(entity.UserId);
        }

        public async Task<CitizenInfoDTO> GetCitizenInfoByUserId(int userId)
        {
            var citizenInfo = await _citizenInfoRepository.GetCitizenInfoByUserId(userId);
            CitizenInfoDTO dto = new CitizenInfoDTO
            {
                CitizenId = citizenInfo.CitizenId,
                Address = citizenInfo.Address,
                DayOfBirth = citizenInfo.DayOfBirth,
                FullName = citizenInfo.FullName,
                UserId = citizenInfo.UserId,
                CitiRegisDate = citizenInfo.CitiRegisDate,
                CitiRegisOffice = citizenInfo.CitiRegisOffice,
                ImageUrls = _imageService.GetImagePathsAsync("CitizenInfo", citizenInfo.Id).Result,
                Sex = citizenInfo.Sex,
                Status = citizenInfo.Status
            };        
            return dto;
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
            entity.CitiRegisDate = request.CitiRegisDate;
            entity.CitiRegisOffice = request.CitiRegisOffice;
            entity.Sex = request.Sex;

            // Xử lý hình ảnh: xóa cũ và upload mới
            await _imageService.DeleteImagesAsync("CitizenInfo", entity.Id);

            if (request.Files != null && request.Files.Count > 0)
            {
                var images = await _imageService.UploadImagesAsync(request.Files, "CitizenInfo", entity.Id);
            }

            await _citizenInfoRepository.UpdateCitizenInfo(entity);
        }
    }
}