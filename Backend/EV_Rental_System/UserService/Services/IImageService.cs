using UserService.Models;

namespace UserService.Services
{
    public interface IImageService
    {
        Task<List<Image>> UploadImagesAsync(List<IFormFile> files, string type, int typeId);
        Task<List<string>> GetImagePathsAsync(string type, int typeId);
        Task DeleteImagesAsync(string type, int typeId);
        Task AddImage(Image image);
    }
}