using Microsoft.AspNetCore.Http;
using TwoWheelVehicleService.Models;
using TwoWheelVehicleService.Repositories;

namespace TwoWheelVehicleService.Services
{
    public class ImageService : IImageService
    {
        private readonly IImageRepository _imageRepository;
        private readonly IWebHostEnvironment _env;
        private readonly string _rootFolder; // Fixed syntax

        public ImageService(IImageRepository imageRepository, IWebHostEnvironment env)
        {
            _imageRepository = imageRepository;
            _env = env;
            _rootFolder = Path.Combine(_env.ContentRootPath, "Data", "Vehicles"); // Fixed syntax
        }

        public async Task<List<Image>> UploadImagesAsync(List<IFormFile> files, int modelId) // Fixed signature to match interface
        {
            var uploadedImages = new List<Image>();
            var folderPath = Path.Combine(_rootFolder, "Models"); // Create a subfolder for models

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            foreach (var file in files)
            {
                // Tạo tên file mới để tránh trùng lặp
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Tạo Image entity với ModelId thay vì Type/TypeId
                var img = new Image
                {
                    Url = Path.Combine("Data", "Vehicles", "Models", fileName).Replace("\\", "/"),
                    ModelId = modelId // Use ModelId directly
                };
                uploadedImages.Add(img);
            }

            return uploadedImages;
        }

        public async Task<List<string>> GetImagePathsAsync(int modelId) // Fixed signature
        {
            var images = await _imageRepository.GetImagesByModelId(modelId);
            return images.Select(i => i.Url).ToList();
        }

        public async Task DeleteImagesAsync(int modelId) // Fixed signature
        {
            var images = await _imageRepository.GetImagesByModelId(modelId);
            foreach (var img in images)
            {
                // Xóa file trên server
                var fullPath = Path.Combine(_env.ContentRootPath, img.Url.Replace("/", "\\"));
                if (File.Exists(fullPath))
                    File.Delete(fullPath);

                // Xóa record trong DB
                await _imageRepository.DeleteImage(img.ImageId);
            }
        }

        public async Task AddImage(Image image)
        {
            await _imageRepository.AddImage(image);
        }
    }
}