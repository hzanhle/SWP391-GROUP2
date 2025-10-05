using UserService.Models;

public interface IImageRepository
{
    Task AddImage(Image image);
    Task DeleteImages(string type, int typeId); // Thêm method này
    Task<List<Image>> GetImagesByTypeId(string type, int typeId);
}