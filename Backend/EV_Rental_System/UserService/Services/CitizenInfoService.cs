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
            if (request == null)
                return new ResponseDTO { Message = "Dữ liệu CitizenInfo không hợp lệ" };

            // 1️⃣ Kiểm tra pending record
            var pending = await _citizenInfoRepository.GetPendingCitizenInfo(request.UserId);
            if (pending != null)
                return new ResponseDTO
                {
                    Message = "Đã có bản CitizenInfo đang chờ xác thực. Vui lòng chờ hoặc liên hệ quản trị viên.",
                    Data = pending
                };

            // 2️⃣ Kiểm tra record đã xác thực
            var existing = await _citizenInfoRepository.GetCitizenInfoByUserId(request.UserId);
            if (existing != null)
                return new ResponseDTO
                {
                    Message = "Người dùng đã có CitizenInfo được xác thực.",
                    Data = existing
                };

            // 3️⃣ Tạo bản pending
            var entity = await CreatePendingCitizenInfo(request);

            _logger.LogInformation("User {UserId} submitted new CitizenInfo pending verification.", request.UserId);

            return new ResponseDTO
            {
                Message = "Yêu cầu tạo CitizenInfo đã được gửi. Vui lòng chờ xác thực.",
                Data = entity
            };
        }

        public async Task UpdateCitizenInfo(CitizenInfoRequest request)
        {
            // Không cho tạo thêm khi đang có bản pending
            var pending = await _citizenInfoRepository.GetPendingCitizenInfo(request.UserId);
            if (pending != null)
                throw new InvalidOperationException("Đã có bản CitizenInfo đang chờ xác thực. Không thể cập nhật thêm.");

            await CreatePendingCitizenInfo(request);
            _logger.LogInformation("User {UserId} submitted CitizenInfo update pending verification.", request.UserId);
        }

        public async Task DeleteCitizenInfo(int id)
        {
            await _citizenInfoRepository.DeleteCitizenInfo(id);
            _logger.LogInformation("CitizenInfo {CitizenInfoId} deleted successfully.", id);
        }

        public async Task<CitizenInfoDTO?> GetCitizenInfoByUserId(int userId)
        {
            var citizenInfo = await _citizenInfoRepository.GetCitizenInfoByUserId(userId);
            if (citizenInfo == null) return null;

            return new CitizenInfoDTO
            {
                Id = citizenInfo.Id,
                CitizenId = citizenInfo.CitizenId,
                Address = citizenInfo.Address,
                DayOfBirth = citizenInfo.DayOfBirth,
                FullName = citizenInfo.FullName,
                UserId = citizenInfo.UserId,
                CitiRegisDate = citizenInfo.CitiRegisDate,
                CitiRegisOffice = citizenInfo.CitiRegisOffice,
                ImageUrls = await _imageService.GetImagePathsAsync("CitizenInfo", citizenInfo.Id),
                Sex = citizenInfo.Sex,
                Status = citizenInfo.Status
            };
        }

        public async Task<Notification> SetStatus(int userId, bool isApproved)
        {
            var pendingEntity = await _citizenInfoRepository.GetPendingCitizenInfo(userId);
            if (pendingEntity == null)
                throw new KeyNotFoundException("Không tìm thấy bản CitizenInfo đang chờ xác thực");

            return await ProcessApproval(pendingEntity, isApproved);
        }

        private async Task<CitizenInfo> CreatePendingCitizenInfo(CitizenInfoRequest request)
        {
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
                Status = CitizenStatus.Pending.ToString(),
                IsApproved = false,
                DayCreated = DateTime.Now
            };

            await _citizenInfoRepository.AddCitizenInfo(entity);

            if (request.Files is { Count: > 0 })
                await _imageService.UploadImagesAsync(request.Files, "CitizenInfo", entity.Id);

            return entity;
        }

        private async Task<Notification> ProcessApproval(CitizenInfo pendingEntity, bool isApproved)
        {
            using var transaction = await _citizenInfoRepository.BeginTransactionAsync();

            try
            {
                Notification notification;

                if (isApproved)
                {
                    pendingEntity.Status = CitizenStatus.Approved.ToString();
                    pendingEntity.IsApproved = true;

                    await _citizenInfoRepository.UpdateCitizenInfo(pendingEntity);
                    await _citizenInfoRepository.DeleteOldApprovedRecords(pendingEntity.UserId, pendingEntity.Id);

                    notification = new Notification
                    {
                        UserId = pendingEntity.UserId,
                        Title = "Xác thực thành công",
                        Message = "Thông tin CCCD của bạn đã được xác nhận.",
                        Created = DateTime.Now
                    };
                }
                else
                {
                    pendingEntity.Status = CitizenStatus.Rejected.ToString();
                    await _citizenInfoRepository.UpdateCitizenInfo(pendingEntity);

                    notification = new Notification
                    {
                        UserId = pendingEntity.UserId,
                        Title = "Xác thực thất bại",
                        Message = "Thông tin CCCD của bạn bị từ chối. Vui lòng kiểm tra lại.",
                        Created = DateTime.Now
                    };
                }

                await _notificationService.AddNotification(notification);
                await transaction.CommitAsync();

                _logger.LogInformation("CitizenInfo approval processed for user {UserId}, Approved: {Approved}",
                    pendingEntity.UserId, isApproved);

                return notification;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error processing CitizenInfo approval for user {UserId}", pendingEntity.UserId);
                throw;
            }
        }
    }
}
