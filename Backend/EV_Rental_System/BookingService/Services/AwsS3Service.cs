using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Options;
using BookingService.Models.ModelSettings;

namespace BookingService.Services
{
    public class AwsS3Service : IAwsS3Service
    {
        private readonly AwsS3Settings _settings;
        private readonly ILogger<AwsS3Service> _logger;
        private readonly IAmazonS3 _s3Client;

        public AwsS3Service(IOptions<AwsS3Settings> settings, ILogger<AwsS3Service> logger)
        {
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Khởi tạo S3 client
            _s3Client = new AmazonS3Client(
                _settings.AccessKey,
                _settings.SecretKey,
                RegionEndpoint.GetBySystemName(_settings.Region)
            );

            _logger.LogInformation("☁️ AWS S3 Initialized => Bucket: {Bucket}, Region: {Region}",
                _settings.BucketName, _settings.Region);
        }

        public async Task<string?> UploadFileAsync(Stream fileStream, string fileName, string contentType = "application/pdf")
        {
            try
            {
                // Tạo key (đường dẫn file trong S3)
                var fileKey = $"{_settings.FolderPath}/{fileName}";

                _logger.LogInformation("📤 Uploading file to S3: {FileKey}", fileKey);

                // Tạo request upload
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = fileStream,
                    Key = fileKey,
                    BucketName = _settings.BucketName,
                    ContentType = contentType,
                };

                // Upload file
                var transferUtility = new TransferUtility(_s3Client);
                await transferUtility.UploadAsync(uploadRequest);

                // Tạo URL công khai
                var fileUrl = $"https://{_settings.BucketName}.s3.{_settings.Region}.amazonaws.com/{fileKey}";

                _logger.LogInformation("✅ File uploaded successfully: {Url}", fileUrl);

                return fileUrl;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "❌ AWS S3 Error: {ErrorCode} - {Message}", ex.ErrorCode, ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error uploading file to S3");
                return null;
            }
        }

        public async Task<bool> DeleteFileAsync(string fileName)
        {
            try
            {
                var fileKey = $"{_settings.FolderPath}/{fileName}";

                _logger.LogInformation("🗑️ Deleting file from S3: {FileKey}", fileKey);

                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _settings.BucketName,
                    Key = fileKey
                };

                await _s3Client.DeleteObjectAsync(deleteRequest);

                _logger.LogInformation("✅ File deleted successfully: {FileKey}", fileKey);

                return true;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "❌ AWS S3 Error deleting file: {ErrorCode}", ex.ErrorCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error deleting file from S3");
                return false;
            }
        }

        public async Task<string?> GetPresignedUrlAsync(string fileName, int expirationMinutes = 60)
        {
            try
            {
                var fileKey = $"{_settings.FolderPath}/{fileName}";

                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _settings.BucketName,
                    Key = fileKey,
                    Expires = DateTime.UtcNow.AddMinutes(expirationMinutes)
                };

                var url = await Task.Run(() => _s3Client.GetPreSignedURL(request));

                _logger.LogInformation("🔗 Generated presigned URL for: {FileKey}", fileKey);

                return url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error generating presigned URL");
                return null;
            }
        }
    }
}