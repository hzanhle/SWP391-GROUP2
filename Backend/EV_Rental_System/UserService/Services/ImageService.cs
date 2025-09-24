using Microsoft.AspNetCore.Http;
using UserService.Models;
using UserService.Repositories;

namespace UserService.Services
{
    public class ImageService : IImageService
    {
        private readonly IImageRepository _imageRepository;
        private readonly IWebHostEnvironment _env;
        // Root folder lưu file (ví dụ: Data/Account)
        private readonly string _rootFolder;

        public ImageService(IImageRepository imageRepository, IWebHostEnvironment env)
        {
            _imageRepository = imageRepository;
            _env = env;
            _rootFolder = Path.Combine(_env.ContentRootPath, "Data", "Account"); // Fixed syntax
        }

        public async Task<List<Image>> UploadImagesAsync(List<IFormFile> files, string type, int typeId)
        {
            var uploadedImages = new List<Image>();
            var folderPath = Path.Combine(_rootFolder, type);

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

                // Tạo Image entity
                var img = new Image
                {
                    Url = Path.Combine("Data", "Account", type, fileName).Replace("\\", "/"), // Normalize path separators
                    Type = type,
                    TypeId = typeId
                };
                uploadedImages.Add(img);
            }

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