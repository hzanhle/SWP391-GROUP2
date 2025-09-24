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
            // 1. Tạo entity
            var entity = new DriverLicense
            {
                UserId = request.UserId,
                LicenseId = request.LicenseId,
                LicenseType = request.LicenseType,
                RegisterDate = request.RegisterDate,
                RegisterOffice = request.RegisterOffice
            };

            // 2. Lưu để EF Core gán Id
            await _driverLicenseRepository.AddDriverLicense(entity);

            // 3. Upload hình nếu FE gửi file nhị phân
            if (request.Files != null && request.Files.Count > 0)
            {
                var images = await _imageService.UploadImagesAsync(request.Files, "DriverLicense", entity.Id);
                // Không cần gán lại entity.Images vì đã có navigation property
                // Chỉ cần add từng image vào database
                foreach (var img in images)
                {
                    await _imageService.AddImage(img);
                }
            }
        }

        public async Task<DriverLicense> GetDriverLicenseByUserId(int userId)
            => await _driverLicenseRepository.GetDriverLicenseByUserId(userId);

        public async Task SetStatus(int userId)
        {
            var entity =  await _driverLicenseRepository.GetDriverLicenseByUserId(userId);
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
                foreach (var img in images)
                {
                    await _imageService.AddImage(img);
                }
            }

            // 3. Cập nhật entity
            await _driverLicenseRepository.UpdateDriverLicense(entity);
        }
    }
}
