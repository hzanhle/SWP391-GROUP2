using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Options;
using BookingService.Models;

namespace BookingService.Services
{
    public class GoogleDriveService : IGoogleDriveService
    {
        private readonly GoogleDriveSettings _settings;
        private readonly ILogger<GoogleDriveService> _logger;
        private readonly DriveService _driveService;

        public GoogleDriveService(
            IOptions<GoogleDriveSettings> settings,
            ILogger<GoogleDriveService> logger)
        {
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Khởi tạo Drive Service
            _driveService = InitializeDriveService();
        }

        /// <summary>
        /// Upload file lên Google Drive và trả về shareable link
        /// </summary>
        public async Task<string?> UploadFileAsync(string filePath, string fileName)
        {
            try
            {
                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogError("File không tồn tại: {FilePath}", filePath);
                    return null;
                }

                _logger.LogInformation("Bắt đầu upload file {FileName} lên Google Drive", fileName);

                // Metadata của file
                var fileMetadata = new Google.Apis.Drive.v3.Data.File
                {
                    Name = fileName,
                    Parents = new List<string> { _settings.ContractsFolderId }
                };

                FilesResource.CreateMediaUpload request;
                await using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    request = _driveService.Files.Create(
                        fileMetadata,
                        stream,
                        "application/pdf"
                    );
                    request.Fields = "id, webViewLink, webContentLink";

                    await request.UploadAsync();
                }

                var uploadedFile = request.ResponseBody;
                if (uploadedFile == null)
                {
                    _logger.LogError("Upload thất bại, không nhận được response");
                    return null;
                }

                _logger.LogInformation("Upload thành công. File ID: {FileId}", uploadedFile.Id);

                // Cấp quyền public view
                await MakeFilePublicAsync(uploadedFile.Id);

                // Trả về link xem file
                var shareableLink = $"https://drive.google.com/file/d/{uploadedFile.Id}/view";
                _logger.LogInformation("Shareable link: {Link}", shareableLink);

                return shareableLink;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi upload file {FileName} lên Google Drive", fileName);
                return null;
            }
        }

        /// <summary>
        /// Xóa file trên Google Drive
        /// </summary>
        public async Task<bool> DeleteFileAsync(string fileId)
        {
            try
            {
                _logger.LogInformation("Xóa file {FileId} trên Google Drive", fileId);

                await _driveService.Files.Delete(fileId).ExecuteAsync();

                _logger.LogInformation("Đã xóa file {FileId} thành công", fileId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa file {FileId}", fileId);
                return false;
            }
        }

        #region Private Methods

        private DriveService InitializeDriveService()
        {
            try
            {
                GoogleCredential credential;

                using (var stream = new FileStream(_settings.ServiceAccountKeyPath, FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream)
                        .CreateScoped(DriveService.ScopeConstants.DriveFile);
                }

                var service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Booking Service"
                });

                _logger.LogInformation("Khởi tạo Google Drive Service thành công");
                return service;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khởi tạo Google Drive Service");
                throw;
            }
        }

        /// <summary>
        /// Cấp quyền "Anyone with link" - Chỉ người có link mới xem được
        /// </summary>
        private async Task MakeFilePublicAsync(string fileId)
        {
            try
            {
                var permission = new Permission
                {
                    Type = "anyone",           // Hoặc "anyoneWithLink" (cùng hiệu quả)
                    Role = "reader",           // Chỉ xem, không edit
                    AllowFileDiscovery = false // ⭐ QUAN TRỌNG: Không cho tìm kiếm trên Drive
                };

                await _driveService.Permissions.Create(permission, fileId).ExecuteAsync();

                _logger.LogInformation("Đã cấp quyền 'Anyone with link' cho file {FileId}", fileId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không thể cấp quyền cho file {FileId}", fileId);
            }
        }

        #endregion
    }
}