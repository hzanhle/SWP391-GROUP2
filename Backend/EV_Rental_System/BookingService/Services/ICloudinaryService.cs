namespace BookingService.Services
{
    public interface ICloudinaryService
    {
        Task<string?> UploadFileAsync(string filePath, string fileName);
        Task<bool> DeleteFileAsync(string publicId);
    }
}
