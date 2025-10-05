using Microsoft.AspNetCore.Http;
using TwoWheelVehicleService.Models;

namespace TwoWheelVehicleService.Services
{
    public interface IImageService
    {
        Task<List<Image>> UploadImagesAsync(List<IFormFile> files, int modelId);
        Task<List<string>> GetImagePathsAsync(int modelId);
        Task DeleteImagesAsync(int modelId);
        Task AddImage(Image image);
    }
}