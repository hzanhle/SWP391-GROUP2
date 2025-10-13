using Microsoft.Extensions.Logging;
using UserService.DTOs;
using UserService.Models;
using UserService.Models.Enums;
using UserService.Models.UserService.Models;
using UserService.Repositories;

namespace UserService.Services
{
    public class CitizenInfoService : ICitizenInfoService
    {
        private readonly ICitizenInfoRepository _citizenInfoRepository;
        private readonly IImageService _imageService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<CitizenInfoService> _logger;

        public CitizenInfoService(
            ICitizenInfoRepository citizenInfoRepository,
            IImageService imageService,
            INotificationService notificationService,
            ILogger<CitizenInfoService> logger)
        {
            _citizenInfoRepository = citizenInfoRepository;
            _imageService = imageService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<ResponseDTO> AddCitizenInfo(CitizenInfoRequest request)
        {
            try
            {
                if (request == null)
                    return new ResponseDTO { Message = "Dữ liệu Căn cước công dân không hợp lệ" };

                var pending = await _citizenInfoRepository.GetPendingCitizenInfo(request.UserId);
                if (pending != null)
                    return new ResponseDTO
                    {
                        Message = "Đã có một bản Căn cước công dân đang chờ xác thực",
                        Data = pending
                    };

                var existing = await _citizenInfoRepository.GetCitizenInfoByUserId(request.UserId);
                if (existing != null)
                    return new ResponseDTO
                    {
                        Message = "Người dùng đã có Căn cước công dân",
                        Data = existing
                    };

                if (request.Files == null || request.Files.Count == 0)
                    return new ResponseDTO
                    {
                        Message = "Vui lòng tải lên ít nhất một hình ảnh của Căn cước công dân"
                    };

                var entity = await CreatePendingCitizenInfo(request);

                return new ResponseDTO
                {
                    Message = "Yêu cầu tạo Căn cước công dân đã được gửi. Vui lòng chờ xác thực",
                    Data = entity
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddCitizenInfo for UserId {UserId}", request?.UserId);
                return new ResponseDTO { Message = $"Lỗi: {ex.Message}" };
            }
        }

        public async Task<CitizenInfoDTO> GetCitizenInfoByUserId(int userId)
        {
            try
            {
                var entity = await _citizenInfoRepository.GetCitizenInfoByUserId(userId);
                if (entity == null) return null;

                return new CitizenInfoDTO
                {
                    Id = entity.Id,
                    UserId = entity.UserId,
                    CitizenId = entity.CitizenId,
                    FullName = entity.FullName,
                    DayOfBirth = entity.DayOfBirth,
                    Sex = entity.Sex,
                    Address = entity.Address,
                    CitiRegisDate = entity.CitiRegisDate,
                    CitiRegisOffice = entity.CitiRegisOffice,
                    Status = entity.Status,
                    ImageUrls = await _imageService.GetImagePathsAsync("CitizenInfo", entity.Id)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCitizenInfoByUserId for UserId {UserId}", userId);
                return null;
            }
        }

        public async Task UpdateCitizenInfo(CitizenInfoRequest request)
        {
            try
            {
                await CreatePendingCitizenInfo(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateCitizenInfo for UserId {UserId}", request?.UserId);
                throw;
            }
        }

        public async Task<Notification> SetStatus(int userId, bool isApproved)
        {
            try
            {
                var pendingEntity = await _citizenInfoRepository.GetPendingCitizenInfo(userId);
                if (pendingEntity == null)
                    throw new Exception("Không tìm thấy bản CitizenInfo đang chờ xác thực");

                return await ProcessApproval(pendingEntity, isApproved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SetStatus for UserId {UserId}", userId);
                throw;
            }
        }

        public async Task DeleteCitizenInfo(int id)
        {
            try
            {
                await _citizenInfoRepository.DeleteCitizenInfo(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteCitizenInfo for Id {Id}", id);
                throw;
            }
        }

        private async Task<CitizenInfo> CreatePendingCitizenInfo(CitizenInfoRequest request)
        {
            var entity = new CitizenInfo
            {
                UserId = request.UserId,
                CitizenId = request.CitizenId,
                FullName = request.FullName,
                DayOfBirth = request.DayOfBirth,
                Sex = request.Sex,
                Address = request.Address,
                CitiRegisDate = request.CitiRegisDate,
                CitiRegisOffice = request.CitiRegisOffice,
                Status = StatusInformation.Pending,
                IsApproved = false,
                DayCreated = DateTime.Now
            };

            await _citizenInfoRepository.AddCitizenInfo(entity);

            if (request.Files != null && request.Files.Count > 0)
                await _imageService.UploadImagesAsync(request.Files, "CitizenInfo", entity.Id);

            return entity;
        }

        private async Task<Notification> ProcessApproval(CitizenInfo pendingEntity, bool isApproved)
        {
            Notification notification;

            if (isApproved)
            {
                pendingEntity.Status = StatusInformation.Approved;
                pendingEntity.IsApproved = true;
                await _citizenInfoRepository.UpdateCitizenInfo(pendingEntity);

                await _citizenInfoRepository.DeleteOldApprovedRecords(pendingEntity.UserId, pendingEntity.Id);

                notification = new Notification
                {
                    UserId = pendingEntity.UserId,
                    Title = "Xác thực thành công",
                    Message = "Thông tin Căn cước công dân của bạn đã được xác nhận",
                    Created = DateTime.Now
                };
            }
            else
            {
                pendingEntity.Status = StatusInformation.Rejected;
                pendingEntity.IsApproved = false;
                await _citizenInfoRepository.UpdateCitizenInfo(pendingEntity);

                notification = new Notification
                {
                    UserId = pendingEntity.UserId,
                    Title = "Xác thực thất bại",
                    Message = "Thông tin Căn cước công dân của bạn bị từ chối. Vui lòng kiểm tra lại thông tin",
                    Created = DateTime.Now
                };
            }

            await _notificationService.AddNotification(notification);
            return notification;
        }
    }
}