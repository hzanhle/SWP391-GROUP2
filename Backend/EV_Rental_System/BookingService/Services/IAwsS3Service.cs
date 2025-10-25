namespace BookingService.Services
{
    public interface IAwsS3Service
    {
        /// <summary>
        /// Upload file lên S3 và trả về URL public
        /// </summary>
        Task<string?> UploadFileAsync(Stream fileStream, string fileName, string contentType = "application/pdf");

        /// <summary>
        /// Xóa file trên S3
        /// </summary>
        Task<bool> DeleteFileAsync(string fileName);

        /// <summary>
        /// Lấy presigned URL (link tạm thời) để download
        /// </summary>
        Task<string?> GetPresignedUrlAsync(string fileName, int expirationMinutes = 60);
    }
}
