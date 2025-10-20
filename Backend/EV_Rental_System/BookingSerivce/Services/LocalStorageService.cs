namespace BookingSerivce.Services
{
    /// <summary>
    /// Local file system storage implementation.
    /// In production, consider using Azure Blob Storage, AWS S3, or similar cloud storage.
    /// </summary>
    public class LocalStorageService : IStorageService
    {
        private readonly string _storagePath;
        private readonly ILogger<LocalStorageService> _logger;
        private readonly IConfiguration _configuration;

        public LocalStorageService(
            ILogger<LocalStorageService> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            // Get storage path from configuration or use default
            _storagePath = _configuration["Storage:ContractsPath"] ?? "wwwroot/contracts";

            // Create directory if it doesn't exist
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
                _logger.LogInformation("Created contracts storage directory at {Path}", _storagePath);
            }
        }

        public async Task<string> UploadContractAsync(string fileName, byte[] fileBytes)
        {
            try
            {
                // Ensure the file name doesn't contain directory traversal attempts
                fileName = Path.GetFileName(fileName);

                var fullPath = Path.Combine(_storagePath, fileName);

                // Create subdirectories if needed (e.g., for year-based organization)
                var directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Write file asynchronously
                await File.WriteAllBytesAsync(fullPath, fileBytes);

                _logger.LogInformation("Uploaded contract file to {Path}", fullPath);

                // Return relative path for database storage
                return $"/contracts/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload contract file {FileName}", fileName);
                throw new IOException($"Failed to upload contract file: {fileName}", ex);
            }
        }

        public async Task<byte[]> DownloadContractAsync(string filePath)
        {
            try
            {
                // Remove leading slash and "contracts/" prefix if present
                filePath = filePath.TrimStart('/').Replace("contracts/", "");

                var fullPath = Path.Combine(_storagePath, filePath);

                if (!File.Exists(fullPath))
                {
                    throw new FileNotFoundException($"Contract file not found: {filePath}");
                }

                return await File.ReadAllBytesAsync(fullPath);
            }
            catch (FileNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download contract file {FilePath}", filePath);
                throw new IOException($"Failed to download contract file: {filePath}", ex);
            }
        }

        public async Task DeleteContractAsync(string filePath)
        {
            try
            {
                // Remove leading slash and "contracts/" prefix if present
                filePath = filePath.TrimStart('/').Replace("contracts/", "");

                var fullPath = Path.Combine(_storagePath, filePath);

                if (File.Exists(fullPath))
                {
                    await Task.Run(() => File.Delete(fullPath));
                    _logger.LogInformation("Deleted contract file {Path}", fullPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete contract file {FilePath}", filePath);
                throw new IOException($"Failed to delete contract file: {filePath}", ex);
            }
        }

        public async Task<bool> FileExistsAsync(string filePath)
        {
            try
            {
                // Remove leading slash and "contracts/" prefix if present
                filePath = filePath.TrimStart('/').Replace("contracts/", "");

                var fullPath = Path.Combine(_storagePath, filePath);
                return await Task.FromResult(File.Exists(fullPath));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if contract file exists {FilePath}", filePath);
                return false;
            }
        }
    }
}
