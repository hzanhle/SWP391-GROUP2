namespace BookingSerivce.Services
{
    public interface IStorageService
    {
        /// <summary>
        /// Uploads a contract PDF file to storage.
        /// </summary>
        /// <param name="fileName">The name of the file (e.g., "contracts/CT-2025-00001.pdf")</param>
        /// <param name="fileBytes">The PDF file content as byte array</param>
        /// <returns>The public URL or path to access the file</returns>
        Task<string> UploadContractAsync(string fileName, byte[] fileBytes);

        /// <summary>
        /// Downloads a contract PDF file from storage.
        /// </summary>
        /// <param name="filePath">The file path or URL</param>
        /// <returns>The file content as byte array</returns>
        Task<byte[]> DownloadContractAsync(string filePath);

        /// <summary>
        /// Deletes a contract PDF file from storage.
        /// </summary>
        /// <param name="filePath">The file path or URL</param>
        Task DeleteContractAsync(string filePath);

        /// <summary>
        /// Checks if a file exists in storage.
        /// </summary>
        Task<bool> FileExistsAsync(string filePath);
    }
}
