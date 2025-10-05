using UserService.DTOs;
using UserService.Models;
using UserService.Repositories;

namespace UserService.Services
{
    public class DriverLicenseService : IDriverLicenseService
    {
        private readonly IDriverLicenseRepository _driverLicenseRepository;
        private readonly IImageService _imageService;
        private readonly INotificationService _notificationService;

        public DriverLicenseService(
            IDriverLicenseRepository driverLicenseRepository,
            IImageService imageService,
            INotificationService notificationService)
        {
            _driverLicenseRepository = driverLicenseRepository;
            _imageService = imageService;
            _notificationService = notificationService;
        }

        public async Task<ResponseDTO> AddDriverLicense(DriverLicenseRequest request)
        {
            if (request == null)
            {
                return new ResponseDTO
                {
                    Message = "Dữ liệu Giấy phép lái xe không hợp lệ",
                    Data = null
                };
            }

            // Kiểm tra nếu có giấy phép đang chờ
            var pending = await _driverLicenseRepository.GetPendingDriverLicense(request.UserId);
            if (pending != null)
            {
                return new ResponseDTO
                {
                    Message = "Đã có một bản Giấy phép lái xe đang chờ xác thực. Vui lòng chờ hoặc liên hệ quản trị viên",
                    Data = pending
                };
            }

            // Kiểm tra nếu đã có giấy phép xác thực
            var existing = await _driverLicenseRepository.GetDriverLicenseByUserId(request.UserId);
            if (existing != null)
            {
                return new ResponseDTO
                {
                    Message = "Người dùng đã có Giấy phép lái xe.",
                    Data = existing
                };
            }

            // Kiểm tra file upload
            if (request.Files == null || request.Files.Count == 0)
            {
                return new ResponseDTO
                {
                    Message = "Vui lòng tải lên ít nhất một hình ảnh của Giấy phép lái xe",
                    Data = null
                };
            }

            // Tạo mới giấy phép
            var entity = await CreatePendingDriverLicense(request);

            return new ResponseDTO
            {
                Message = "Yêu cầu tạo Giấy phép lái xe đã được gửi. Vui lòng chờ xác thực.",
                Data = entity
            };
        }

        public async Task<DriverLicenseDTO> GetDriverLicenseByUserId(int userId)
        {
            var entity = await _driverLicenseRepository.GetDriverLicenseByUserId(userId);
            if (entity == null) return null;

            var dto = new DriverLicenseDTO
            {
                Id = entity.Id,
                UserId = entity.UserId,
                LicenseId = entity.LicenseId,
                LicenseType = entity.LicenseType,
                RegisterDate = entity.RegisterDate,
                RegisterOffice = entity.RegisterOffice,
                Status = entity.Status,
                ImageUrls = await _imageService.GetImagePathsAsync("DriverLicense", entity.Id)
            };
            return dto;
        }

        public async Task UpdateDriverLicense(DriverLicenseRequest request)
        {
            await CreatePendingDriverLicense(request);
        }

        public async Task<Notification> SetStatus(int userId, bool isApproved)
        {
            var pendingEntity = await _driverLicenseRepository.GetPendingDriverLicense(userId);
            if (pendingEntity == null)
                throw new Exception("Không tìm thấy bản DriverLicense đang chờ xác thực");

            return await ProcessApproval(pendingEntity, isApproved);
        }

        public async Task DeleteDriverLicense(int id)
        {
            await _driverLicenseRepository.DeleteDriverLicense(id);
        }

        // ==================== private helpers ====================

        private async Task<DriverLicense> CreatePendingDriverLicense(DriverLicenseRequest request)
        {
            var entity = new DriverLicense
            {
                UserId = request.UserId,
                LicenseId = request.LicenseId,
                LicenseType = request.LicenseType,
                RegisterDate = request.RegisterDate,
                RegisterOffice = request.RegisterOffice,
                Status = "Chờ Xác Thực",
                IsApproved = false,
                DateCreated = DateTime.Now
            };

            await _driverLicenseRepository.AddDriverLicense(entity);

            if (request.Files != null && request.Files.Count > 0)
            {
                await _imageService.UploadImagesAsync(request.Files, "DriverLicense", entity.Id);
            }

            return entity;
        }

        private async Task<Notification> ProcessApproval(DriverLicense pendingEntity, bool isApproved)
        {
            Notification notification;

            if (isApproved)
            {
                pendingEntity.Status = "Đã xác nhận";
                pendingEntity.IsApproved = true;
                await _driverLicenseRepository.UpdateDriverLicense(pendingEntity);

                // Xóa bản cũ nhất đã xác nhận trước đó
                await _driverLicenseRepository.DeleteOldApprovedRecords(pendingEntity.UserId, pendingEntity.Id);

                notification = new Notification
                {
                    UserId = pendingEntity.UserId,
                    Title = "Xác thực thành công",
                    Message = "Thông tin Giấy phép lái xe của bạn đã được xác nhận",
                    Created = DateTime.Now
                };
            }
            else
            {
                pendingEntity.Status = "Bị từ chối";
                pendingEntity.IsApproved = false;
                await _driverLicenseRepository.UpdateDriverLicense(pendingEntity);

                notification = new Notification
                {
                    UserId = pendingEntity.UserId,
                    Title = "Xác thực thất bại",
                    Message = "Thông tin Giấy phép lái xe của bạn bị từ chối. Vui lòng kiểm tra lại thông tin",
                    Created = DateTime.Now
                };
            }

            await _notificationService.AddNotification(notification);
            return notification;
        }
    }
}
