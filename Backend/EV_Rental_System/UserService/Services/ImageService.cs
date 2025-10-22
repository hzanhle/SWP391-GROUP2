using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Models;
using UserService.Repositories;

namespace UserService.Services
{
    public class ImageService : IImageService
    {
        private readonly IImageRepository _imageRepository;
        private readonly IWebHostEnvironment _env;
        private readonly string _rootFolder;
        private readonly ILogger<ImageService> _logger;

        public ImageService(IImageRepository imageRepository, IWebHostEnvironment env, ILogger<ImageService> logger)
        {
            _imageRepository = imageRepository;
            _env = env;
            _logger = logger;

            _rootFolder = Path.Combine(_env.ContentRootPath, "Data", "Account");

            _logger.LogInformation("🗂️ ContentRootPath: {ContentRootPath}", _env.ContentRootPath);
            _logger.LogInformation("🗂️ Root Folder: {RootFolder}", _rootFolder);
        }

        public async Task<List<Image>> UploadImagesAsync(List<IFormFile> files, string type, int typeId)
        {
            _logger.LogInformation("🔍 UploadImagesAsync called: files={Count}, type={Type}, typeId={TypeId}",
                files?.Count, type, typeId);

            if (files == null || !files.Any())
            {
                _logger.LogWarning("❌ No files to upload");
                return new List<Image>();
            }

            var uploadedImages = new List<Image>();
            var folderPath = Path.Combine(_rootFolder, type);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                _logger.LogInformation("📁 Created directory: {FolderPath}", folderPath);
            }

            foreach (var file in files)
            {
                if (file.Length == 0) continue;

                if (!IsValidImageFile(file))
                {
                    var msg = $"Invalid file type: {file.FileName}";
                    _logger.LogWarning(msg);
                    throw new ArgumentException(msg);
                }

                var fileName = $"{typeId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(folderPath, fileName);

                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    _logger.LogInformation("✅ Saved file successfully: {FilePath}, size={FileSize} bytes",
                        filePath, new FileInfo(filePath).Length);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Failed to save file: {FilePath}", filePath);
                    throw;
                }

                var imageUrl = Path.Combine("Data", "Account", type, fileName).Replace("\\", "/");
                var image = new Image(imageUrl, type, typeId);
                _logger.LogInformation("🏷️ Image URL: {Url}, File: {FileName}", imageUrl, fileName);

                uploadedImages.Add(image);
            }

            if (uploadedImages.Any())
            {
                foreach (var image in uploadedImages)
                {
                    await _imageRepository.AddImage(image);
                    _logger.LogInformation("✅ Saved image to DB: {Url}", image.Url);
                }
            }

            _logger.LogInformation("🎉 Upload completed: {Count} images", uploadedImages.Count);
            return uploadedImages;
        }

        public async Task<List<string>> GetImagePathsAsync(string type, int typeId)
        {
            var images = await _imageRepository.GetImagesByTypeId(type, typeId);
            return images.Select(i => i.Url).ToList();
        }

        public async Task DeleteImagesAsync(string type, int typeId)
        {
            var images = await _imageRepository.GetImagesByTypeId(type, typeId);

            if (!images.Any()) return;

            foreach (var img in images)
            {
                try
                {
                    var fullPath = Path.Combine(_env.ContentRootPath, img.Url.Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (File.Exists(fullPath))
                        File.Delete(fullPath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete file {Url}", img.Url);
                }
            }

            await _imageRepository.DeleteImages(type, typeId);
        }

        public async Task AddImage(Image image)
        {
            await _imageRepository.AddImage(image);
        }

        private bool IsValidImageFile(IFormFile file)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var isValid = allowedExtensions.Contains(extension);

            _logger.LogInformation("🔍 File validation: {FileName}, Extension: {Extension}, IsValid: {IsValid}",
                file.FileName, extension, isValid);

            return isValid;
        }
    }
}
