using TwoWheelVehicleService.Models;
using TwoWheelVehicleService.Repositories;

namespace TwoWheelVehicleService.Services
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
            _rootFolder = Path.Combine(_env.ContentRootPath, "Data", "Vehicles");
        }

        public async Task<List<Image>> UploadImagesAsync(List<IFormFile> files, int modelId)
        {
            var uploadedImages = new List<Image>();

            try
            {
                var folderPath = Path.Combine(_rootFolder, "Models");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    _logger.LogInformation("📁 Created image directory at: {FolderPath}", folderPath);
                }

                foreach (var file in files)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    var filePath = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var img = new Image
                    {
                        Url = Path.Combine("Data", "Vehicles", "Models", fileName).Replace("\\", "/"),
                        ModelId = modelId
                    };

                    uploadedImages.Add(img);
                    _logger.LogInformation("✅ Uploaded image for ModelId={ModelId}: {FileName}", modelId, fileName);
                }

                return uploadedImages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error while uploading images for ModelId={ModelId}", modelId);
                throw;
            }
        }

        public async Task<List<string>> GetImagePathsAsync(int modelId)
        {
            try
            {
                var images = await _imageRepository.GetImagesByModelId(modelId);
                var urls = images.Select(i => i.Url).ToList();

                _logger.LogInformation("📸 Retrieved {Count} image paths for ModelId={ModelId}", urls.Count, modelId);
                return urls;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error while retrieving image paths for ModelId={ModelId}", modelId);
                throw;
            }
        }

        public async Task DeleteImagesAsync(int modelId)
        {
            try
            {
                var images = await _imageRepository.GetImagesByModelId(modelId);
                if (!images.Any())
                {
                    _logger.LogWarning("⚠ No images found to delete for ModelId={ModelId}", modelId);
                    return;
                }

                foreach (var img in images)
                {
                    var fullPath = Path.Combine(_env.ContentRootPath, img.Url.Replace("/", "\\"));
                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                        _logger.LogInformation("🗑️ Deleted file from disk: {FilePath}", fullPath);
                    }
                    else
                    {
                        _logger.LogWarning("⚠ File not found when attempting to delete: {FilePath}", fullPath);
                    }

                    await _imageRepository.DeleteImage(img.ImageId);
                    _logger.LogInformation("✅ Deleted image record from DB: ImageId={ImageId}", img.ImageId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error while deleting images for ModelId={ModelId}", modelId);
                throw;
            }
        }

        public async Task AddImage(Image image)
        {
            try
            {
                await _imageRepository.AddImage(image);
                _logger.LogInformation("✅ Added image record successfully: {@Image}", image);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error while adding image record: {@Image}", image);
                throw;
            }
        }
    }
}
