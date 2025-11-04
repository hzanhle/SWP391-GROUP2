using Microsoft.Extensions.Logging;
using UserService.DTOs;
using UserService.Models;
using UserService.Models.Enums;
using UserService.Repositories;

namespace UserService.Services
{
    public class DriverLicenseService : IDriverLicenseService
    {
        private readonly IDriverLicenseRepository _driverLicenseRepository;
        private readonly IImageService _imageService;
        private readonly ICitizenInfoService _citizenInfoService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<DriverLicenseService> _logger;

        public DriverLicenseService(
            IDriverLicenseRepository driverLicenseRepository,
            ICitizenInfoService citizenInfoService,
            IImageService imageService,
            INotificationService notificationService,
            ILogger<DriverLicenseService> logger)
        {
            _driverLicenseRepository = driverLicenseRepository;
            _imageService = imageService;
            _citizenInfoService = citizenInfoService;
            _notificationService = notificationService;
        }

        public async Task<ResponseDTO> AddDriverLicense(DriverLicenseRequest request, int userId)
        {
            try
            {
                if (request == null)
                    return new ResponseDTO { Message = "Dữ liệu Giấy phép lái xe không hợp lệ" };

                var pending = await _driverLicenseRepository.GetPendingDriverLicense(userId);
                if (pending != null)
                    return new ResponseDTO
                    {
                        Message = "Đã có một bản Giấy phép lái xe đang chờ xác thực",
                        Data = pending
                    };

                var existing = await _driverLicenseRepository.GetDriverLicenseByUserId(userId);
                if (existing != null)
                    return new ResponseDTO
                    {
                        Message = "Người dùng đã có Giấy phép lái xe",
                        Data = existing
                    };

                if (request.Files == null || request.Files.Count == 0)
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Vui lòng tải lên ít nhất một hình ảnh của Giấy phép lái xe",
                        Data = request
                    };

                var entity = await CreatePendingDriverLicense(request, userId);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Yêu cầu tạo Giấy phép lái xe đã được gửi. Vui lòng chờ xác thực",
                    Data = entity
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddDriverLicense for UserId {UserId}", userId);
                return new ResponseDTO { Message = $"Lỗi: {ex.Message}" };
            }
        }

        public async Task<DriverLicenseDTO> GetDriverLicenseByUserId(int userId)
        {
            try
            {
                var entity = await _driverLicenseRepository.GetDriverLicenseByUserId(userId);
                if (entity == null) return null;

                return new DriverLicenseDTO
                {
                    Id = entity.Id,
                    UserId = entity.UserId,
                    LicenseId = entity.LicenseId,
                    FullName = entity.FullName,
                    Address = entity.Address,
                    Sex = entity.Sex,
                    DayOfBirth = entity.DayOfBirth,
                    LicenseType = entity.LicenseType,
                    RegisterDate = entity.RegisterDate,
                    RegisterOffice = entity.RegisterOffice,
                    Status = entity.Status,
                    ImageUrls = await _imageService.GetImagePathsAsync("DriverLicense", entity.Id)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDriverLicenseByUserId for UserId {UserId}", userId);
                return null;
            }
        }

        public async Task UpdateDriverLicense(DriverLicenseRequest request, int userId)
        {
            try
            {
                await CreatePendingDriverLicense(request, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateDriverLicense for UserId {UserId}", userId);
                throw;
            }
        }

        public async Task<Notification> SetStatus(int userId, bool isApproved)
        {
            try
            {
                var pendingEntity = await _driverLicenseRepository.GetPendingDriverLicense(userId);
                if (pendingEntity == null)
                    throw new Exception("Không tìm thấy bản DriverLicense đang chờ xác thực");

                return await ProcessApproval(pendingEntity, isApproved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SetStatus for UserId {UserId}", userId);
                throw;
            }
        }

        public async Task DeleteDriverLicense(int id)
        {
            try
            {
                await _driverLicenseRepository.DeleteDriverLicense(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteDriverLicense for Id {Id}", id);
                throw;
            }
        }

        private async Task<DriverLicense> CreatePendingDriverLicense(DriverLicenseRequest request, int userId)
        {
            try
            {
                var entity = new DriverLicense
                {
                    FullName = request.FullName,
                    UserId = userId,
                    LicenseId = request.LicenseId,
                    LicenseType = request.LicenseType,
                    Address = request.Address,
                    Sex = request.Sex,
                    DayOfBirth = request.DayOfBirth,
                    RegisterDate = request.RegisterDate,
                    RegisterOffice = request.RegisterOffice,
                    Status = StatusInformation.Pending,
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
            catch (Exception ex)
            {
                // Log nếu có ILogger hoặc dùng Console
                Console.WriteLine($"❌ Error in CreatePendingDriverLicense: {ex.Message}");
                // Ném tiếp để caller xử lý
                throw;
            }
        }



        private async Task<Notification> ProcessApproval(DriverLicense pendingEntity, bool isApproved)
        {
            Notification notification;

            if (isApproved)
            {
                pendingEntity.Status = StatusInformation.Approved;
                pendingEntity.IsApproved = true;
                await _driverLicenseRepository.UpdateDriverLicense(pendingEntity);

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
                pendingEntity.Status = StatusInformation.Rejected;
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
