namespace BookingService.Services
{
    public interface IGoogleDriveService
    {
        Task<string?> UploadFileAsync(string filePath, string fileName);
        Task<bool> DeleteFileAsync(string fileId);
    }
}
