using BookingService.Models;
using BookingService.Services;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryService> _logger;

    public CloudinaryService(IOptions<CloudinarySettings> config, ILogger<CloudinaryService> logger)
    {
        _logger = logger;
        var settings = config.Value;

        // ✅ Tạo tài khoản Cloudinary từ appsettings.json
        var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
        _cloudinary = new Cloudinary(account)
        {
            Api = { Secure = true }
        };
    }

    /// <summary>
    /// Upload file lên Cloudinary (PDF, DOCX, vv)
    /// </summary>
    public async Task<string?> UploadFileAsync(string filePath, string fileName)
    {
        try
        {
            var extension = Path.GetExtension(filePath)?.ToLower();

            // ✅ Nếu là PDF hoặc file khác ảnh → dùng RawUploadParams
            if (extension == ".pdf" || extension == ".docx" || extension == ".zip")
            {
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(filePath),
                    PublicId = Path.GetFileNameWithoutExtension(fileName),
                    Folder = "contracts" // tùy bạn muốn
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                return uploadResult?.SecureUrl?.ToString();
            }
            else
            {
                // ✅ Ảnh bình thường
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(filePath),
                    PublicId = Path.GetFileNameWithoutExtension(fileName),
                    Folder = "images"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                return uploadResult?.SecureUrl?.ToString();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cloudinary upload failed for {File}", filePath);
            return null;
        }
    }

    /// <summary>
    /// Xóa file trên Cloudinary
    /// </summary>
    public async Task<bool> DeleteFileAsync(string publicId)
    {
        try
        {
            _logger.LogInformation("🗑️ Xóa file {PublicId} trên Cloudinary", publicId);

            var deletionParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Raw
            };

            var result = await _cloudinary.DestroyAsync(deletionParams);

            if (result.Result == "ok")
            {
                _logger.LogInformation("✅ Đã xóa file {PublicId} thành công", publicId);
                return true;
            }

            _logger.LogWarning("⚠️ Không thể xóa file {PublicId}. Result: {Result}", publicId, result.Result);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Lỗi xóa file {PublicId}: {Message}", publicId, ex.Message);
            return false;
        }
    }
}