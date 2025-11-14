namespace BookingService.Services
{
    public class ImageStorageService : IImageStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ImageStorageService> _logger;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB

        public ImageStorageService(IWebHostEnvironment environment, ILogger<ImageStorageService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<string> SaveImagesAsync(List<IFormFile> images, string folder = "vehicle-images")
        {
            if (images == null || images.Count == 0)
            {
                return string.Empty;
            }

            var imageUrls = new List<string>();

            foreach (var image in images)
            {
                if (IsValidImage(image))
                {
                    var url = await SaveImageAsync(image, folder);
                    if (!string.IsNullOrEmpty(url))
                    {
                        imageUrls.Add(url);
                    }
                }
                else
                {
                    _logger.LogWarning("Invalid image file skipped: {FileName}", image.FileName);
                }
            }

            return string.Join(";", imageUrls);
        }

        public async Task<string> SaveImageAsync(IFormFile image, string folder = "vehicle-images")
        {
            try
            {
                if (!IsValidImage(image))
                {
                    throw new InvalidOperationException("Invalid image file");
                }

                // Create folder if not exists
                var uploadsFolder = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", folder);
                Directory.CreateDirectory(uploadsFolder);

                // Generate unique filename
                var fileName = image.FileName ?? "image";
                var fileExtension = Path.GetExtension(fileName);
                if (string.IsNullOrEmpty(fileExtension))
                {
                    // Default to .jpg if no extension
                    fileExtension = ".jpg";
                }
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                // Return relative URL
                var relativeUrl = $"/uploads/{folder}/{uniqueFileName}";
                _logger.LogInformation("Image saved successfully: {Url}", relativeUrl);

                return relativeUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving image: {FileName}", image.FileName);
                throw;
            }
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imageUrl))
                {
                    return false;
                }

                // Convert URL to physical path
                var relativePath = imageUrl.TrimStart('/');
                var physicalPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));

                if (File.Exists(physicalPath))
                {
                    await Task.Run(() => File.Delete(physicalPath));
                    _logger.LogInformation("Image deleted successfully: {Url}", imageUrl);
                    return true;
                }

                _logger.LogWarning("Image not found for deletion: {Url}", imageUrl);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image: {Url}", imageUrl);
                return false;
            }
        }

        public async Task<bool> DeleteImagesAsync(string imageUrls)
        {
            if (string.IsNullOrWhiteSpace(imageUrls))
            {
                return false;
            }

            var urls = imageUrls.Split(';', StringSplitOptions.RemoveEmptyEntries);
            var allDeleted = true;

            foreach (var url in urls)
            {
                var deleted = await DeleteImageAsync(url.Trim());
                if (!deleted)
                {
                    allDeleted = false;
                }
            }

            return allDeleted;
        }

        public bool IsValidImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return false;
            }

            // Check file size
            if (file.Length > _maxFileSize)
            {
                _logger.LogWarning("File size exceeds limit: {FileName} ({Size} bytes)", file.FileName ?? "unknown", file.Length);
                return false;
            }

            // Check file extension
            var fileName = file.FileName ?? string.Empty;
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !_allowedExtensions.Contains(extension))
            {
                _logger.LogWarning("Invalid file extension: {FileName}", fileName);
                return false;
            }

            // Check content type
            if (!file.ContentType.StartsWith("image/"))
            {
                _logger.LogWarning("Invalid content type: {ContentType}", file.ContentType);
                return false;
            }

            return true;
        }
    }
}
