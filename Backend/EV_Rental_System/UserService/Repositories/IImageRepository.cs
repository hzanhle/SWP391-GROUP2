using UserService.Models;

public interface IImageRepository
{
    Task AddImage(Image image);
    Task DeleteImages(List<int> imageIds); // Thêm method này
    Task<List<Image>> GetImagesByTypeId(string type, int typeId);
}