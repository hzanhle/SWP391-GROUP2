using TwoWheelVehicleService.Models;

public interface IImageRepository
{
    Task AddImage(Image image);
    Task<List<Image>> GetImagesByModelId(int modelId);
    Task DeleteImage(int imageId);
}
