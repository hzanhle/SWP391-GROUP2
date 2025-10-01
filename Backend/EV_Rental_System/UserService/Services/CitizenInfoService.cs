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
        private readonly INotificationService _notificationService; // Thêm này

        public CitizenInfoService(
            ICitizenInfoRepository citizenInfoRepository,
            IImageService imageService,
            INotificationService notificationService) // Thêm parameter
        {
            _citizenInfoRepository = citizenInfoRepository;
            _imageService = imageService;
            _notificationService = notificationService;
        }

        public async Task<CitizenInfo> AddCitizenInfo(CitizenInfoRequest request)
        {
            // Sử dụng method chung để tạo pending record
            var entity = await CreatePendingCitizenInfo(request);

            // Load lại từ database với Images included
            return await _citizenInfoRepository.GetCitizenInfoByUserId(entity.UserId);
        }

        public async Task DeleteCitizenInfo(int id)
        {
            await _citizenInfoRepository.DeleteCitizenInfo(id);
        }

        public async Task<CitizenInfoDTO> GetCitizenInfoByUserId(int userId)
        {
            var citizenInfo = await _citizenInfoRepository.GetCitizenInfoByUserId(userId);

            if (citizenInfo == null)
                return null;

            CitizenInfoDTO dto = new CitizenInfoDTO
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
            return dto;
        }

        public async Task<Notification> SetStatus(int userId, bool isApproved)
        {
            var pendingEntity = await _citizenInfoRepository.GetPendingCitizenInfo(userId);
            if (pendingEntity == null)
                throw new Exception("Không tìm thấy bản CitizenInfo đang chờ xác thực");

            return await ProcessApproval(pendingEntity, isApproved);
        }

        public async Task UpdateCitizenInfo(CitizenInfoRequest request)
        {
            // Sử dụng method chung để tạo pending record
            await CreatePendingCitizenInfo(request);
        }

        // Method chung để tạo bản ghi pending (dùng cho cả Add và Update)
        private async Task<CitizenInfo> CreatePendingCitizenInfo(CitizenInfoRequest request)
        {
            // Tạo entity CitizenInfo với trạng thái pending
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
                Status = "Chờ Xác Thực",
                IsApproved = false,
                DayCreated = DateTime.Now,
            };

            // Lưu CitizenInfo để EF Core gán Id
            await _citizenInfoRepository.AddCitizenInfo(entity);

            // Upload file và lưu Image nếu FE gửi file nhị phân
            if (request.Files != null && request.Files.Count > 0)
            {
                await _imageService.UploadImagesAsync(request.Files, "CitizenInfo", entity.Id);
            }

            return entity;
        }

        // Method riêng để xử lý logic approve/reject (dùng cho cả Add và Update)
        private async Task<Notification> ProcessApproval(CitizenInfo pendingEntity, bool isApproved)
        {
            Notification notification;

            if (isApproved)
            {
                pendingEntity.Status = "Đã xác nhận";
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