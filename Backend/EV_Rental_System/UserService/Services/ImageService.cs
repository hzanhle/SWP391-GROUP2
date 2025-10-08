using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UserService.Models;
using UserService.Repositories;

namespace UserService.Services
{
    public class ImageService : IImageService
    {
        private readonly IImageRepository _imageRepository;
        private readonly IWebHostEnvironment _env;
        private readonly string _rootFolder;

        public ImageService(IImageRepository imageRepository, IWebHostEnvironment env)
        {
            _imageRepository = imageRepository;
            _env = env;
            _rootFolder = Path.Combine(_env.ContentRootPath, "Data", "Account");

            // ✅ Debug: In ra đường dẫn thực tế
            Console.WriteLine($"🗂️ ContentRootPath: {_env.ContentRootPath}");
            Console.WriteLine($"🗂️ Root Folder: {_rootFolder}");
        }

        public async Task<List<Image>> UploadImagesAsync(List<IFormFile> files, string type, int typeId)
        {
            Console.WriteLine($"🔍 UploadImagesAsync called: files={files?.Count}, type={type}, typeId={typeId}");

            if (files == null || !files.Any())
            {
                Console.WriteLine("❌ No files to upload");
                return new List<Image>();
            }

            var uploadedImages = new List<Image>();
            var folderPath = Path.Combine(_rootFolder, type);

            // Ensure directory exists
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                Console.WriteLine($"📁 Created directory: {folderPath}");
            }

            foreach (var file in files)
            {
                if (file.Length == 0) continue;

                // Validate file type (optional)
                if (!IsValidImageFile(file))
                    throw new ArgumentException($"Invalid file type: {file.FileName}");

                var fileName = $"{typeId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(folderPath, fileName);

                // Save file to disk
                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    Console.WriteLine($"✅ Saved file successfully: {filePath}");
                    Console.WriteLine($"📏 File size: {new FileInfo(filePath).Length} bytes");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Failed to save file: {filePath}");
                    Console.WriteLine($"❌ Error: {ex.Message}");
                    throw;
                }

                // Create Image entity - ✅ SỬA: Dùng đúng fileName có prefix
                var imageUrl = Path.Combine("Data", "Account", type, fileName).Replace("\\", "/");
                var image = new Image(
                    imageUrl,
                    type,
                    typeId
                );
                Console.WriteLine($"🏷️ Image URL: {imageUrl}, File: {fileName}");

                uploadedImages.Add(image);
            }

            // Save all images to database
            if (uploadedImages.Any())
            {
                foreach (var image in uploadedImages)
                {
                    await _imageRepository.AddImage(image);
                    Console.WriteLine($"✅ Saved image to DB: {image.Url}");
                }
            }

            Console.WriteLine($"🎉 Upload completed: {uploadedImages.Count} images");
            return uploadedImages;
        }

        public async Task<List<string>> GetImagePathsAsync(string type, int typeId)
        {
            var images = await _imageRepository.GetImagesByTypeId(type, typeId);
            return images.Select(i => i.Url).ToList();
        }

        public async Task DeleteImagesAsync(string type, int typeId)
        {
            // Lấy tất cả ảnh từ DB
            var images = await _imageRepository.GetImagesByTypeId(type, typeId);

            if (!images.Any()) return; // Không có gì để xóa

            foreach (var img in images)
            {
                try
                {
                    var fullPath = Path.Combine(_env.ContentRootPath, img.Url.Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                    }
                }
                catch (Exception ex)
                {
                    // Log lỗi nếu xóa file thất bại, nhưng không dừng toàn bộ
                    Console.WriteLine($"Failed to delete file {img.Url}: {ex.Message}");
                }
            }

            // Xóa record khỏi DB qua repo
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

            Console.WriteLine($"🔍 File validation: {file.FileName}");
            Console.WriteLine($"🔍 Extension: {extension}");
            Console.WriteLine($"🔍 Is valid: {isValid}");

            return isValid;
        }
    }
}

