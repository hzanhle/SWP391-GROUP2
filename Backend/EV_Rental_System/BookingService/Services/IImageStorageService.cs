namespace BookingService.Services
{
    public interface IImageStorageService
    {
        /// <summary>
        /// Lưu nhiều file ảnh và trả về danh sách URL (phân cách bằng dấu ;)
        /// </summary>
        Task<string> SaveImagesAsync(List<IFormFile> images, string folder = "vehicle-images");

        /// <summary>
        /// Lưu một file ảnh và trả về URL
        /// </summary>
        Task<string> SaveImageAsync(IFormFile image, string folder = "vehicle-images");

        /// <summary>
        /// Xóa ảnh dựa trên URL
        /// </summary>
        Task<bool> DeleteImageAsync(string imageUrl);

        /// <summary>
        /// Xóa nhiều ảnh (URL phân cách bằng dấu ;)
        /// </summary>
        Task<bool> DeleteImagesAsync(string imageUrls);

        /// <summary>
        /// Validate file ảnh
        /// </summary>
        bool IsValidImage(IFormFile file);
    }
}
