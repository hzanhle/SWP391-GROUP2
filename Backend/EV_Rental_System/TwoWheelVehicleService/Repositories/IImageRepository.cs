using TwoWheelVehicleService.Models;

namespace TwoWheelVehicleService.Repositories
{
    public interface IImageRepository
    {
        Task AddImage(Image image);
        Task<List<Image>> GetImagesByModelId(int modelId);
        Task DeleteImage(int imageId);
        Task DeleteImages(List<int> imageIds);
    }
}