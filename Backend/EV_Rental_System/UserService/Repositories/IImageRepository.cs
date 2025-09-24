using UserService.Models;

public interface IImageRepository
{
    Task AddImage(Image image);
    Task<List<Image>> GetImagesByTypeId(string type, int typeId);
    Task DeleteImage(int imageId);
}
