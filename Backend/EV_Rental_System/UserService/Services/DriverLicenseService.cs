using UserService.DTOs;
using UserService.Models;
using UserService.Repositories;

namespace UserService.Services
{
    public class DriverLicenseService : IDriverLicenseService
    {
        private readonly IDriverLicenseRepository _driverLicenseRepository;
        private readonly IImageService _imageService;

        public DriverLicenseService(IDriverLicenseRepository driverLicenseRepository, IImageService imageService)
        {
            _driverLicenseRepository = driverLicenseRepository;
            _imageService = imageService;
        }

        public async Task AddDriverLicense(DriverLicenseRequest request)
        {
            // ✅ Debug: Check files trước khi xử lý
            Console.WriteLine($"🔍 AddDriverLicense called for UserId: {request.UserId}");
            Console.WriteLine($"🔍 Files count: {request.Files?.Count ?? 0}");

            if (request.Files != null)
            {
                for (int i = 0; i < request.Files.Count; i++)
                {
                    var file = request.Files[i];
                    Console.WriteLine($"📎 File {i}: {file.FileName}, Size: {file.Length} bytes");
                }
            }

            // 1. Tạo entity DriverLicense với đầy đủ thông tin
            var entity = new DriverLicense
            {
                UserId = request.UserId,
                LicenseId = request.LicenseId,
                LicenseType = request.LicenseType,
                RegisterDate = request.RegisterDate,
                RegisterOffice = request.RegisterOffice,
                Status = "Chờ Xác Thực"
            };

            // 2. Lưu DriverLicense để EF Core gán Id
            await _driverLicenseRepository.AddDriverLicense(entity);
            Console.WriteLine($"✅ DriverLicense saved with Id: {entity.Id}");

            // 3. Upload file và lưu Image nếu FE gửi file nhị phân
            if (request.Files != null && request.Files.Count > 0)
            {
                Console.WriteLine($"🚀 Starting upload for typeId: {entity.Id}");
                var images = await _imageService.UploadImagesAsync(request.Files, "DriverLicense", entity.Id);
                Console.WriteLine($"🎯 Upload result: {images.Count} images uploaded");
            }
            else
            {
                Console.WriteLine("⚠️ No files to upload");
            }

            //// 4. ✅ Load lại từ database với Images included
            //return await _driverLicenseRepository.GetDriverLicenseByUserId(entity.UserId);
        }

        public async Task<DriverLicenseDTO> GetDriverLicenseByUserId(int userId)
        {
            var entity = await _driverLicenseRepository.GetDriverLicenseByUserId(userId);
            if (entity == null)
                throw new Exception("DriverLicense not found");
            // Map entity to DTO
            var dto = new DriverLicenseDTO
            {
                UserId = entity.UserId,
                LicenseId = entity.LicenseId,
                LicenseType = entity.LicenseType,
                RegisterDate = entity.RegisterDate,
                RegisterOffice = entity.RegisterOffice,
                Status = entity.Status,
                ImageUrls = _imageService.GetImagePathsAsync("DriverLicense", entity.Id).Result
            };
            return dto;
        }

        public async Task SetStatus(int userId)
        {
            var entity = await _driverLicenseRepository.GetDriverLicenseByUserId(userId);
            if (entity == null)
                throw new Exception("DriverLicense not found");

            entity.Status = "Đã xác nhận";
            await _driverLicenseRepository.UpdateDriverLicense(entity);
        }

        public async Task UpdateDriverLicense(DriverLicenseRequest request)
        {
            var entity = await _driverLicenseRepository.GetDriverLicenseByUserId(request.UserId);
            if (entity == null)
                throw new Exception("DriverLicense not found");

            // 1. Cập nhật thông tin cơ bản
            entity.LicenseId = request.LicenseId;
            entity.LicenseType = request.LicenseType;
            entity.RegisterDate = request.RegisterDate;
            entity.RegisterOffice = request.RegisterOffice;

            // 2. Xử lý hình ảnh: xóa cũ và upload mới
            await _imageService.DeleteImagesAsync("DriverLicense", entity.Id);

            if (request.Files != null && request.Files.Count > 0)
            {
                var images = await _imageService.UploadImagesAsync(request.Files, "DriverLicense", entity.Id);
                // Images đã được lưu trong UploadImagesAsync
            }

            // 3. Cập nhật entity
            await _driverLicenseRepository.UpdateDriverLicense(entity);
        }
    }
}