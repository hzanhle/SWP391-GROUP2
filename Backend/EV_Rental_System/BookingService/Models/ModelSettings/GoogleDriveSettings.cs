namespace BookingService.Models
{
    /// <summary>
    /// Cấu hình Google Drive API
    /// </summary>
    public class GoogleDriveSettings
    {
        /// <summary>
        /// Đường dẫn đến file JSON Service Account Key
        /// Ví dụ: "Config/service-account-key.json"
        /// </summary>
        public string ServiceAccountKeyPath { get; set; } = string.Empty;

        /// <summary>
        /// ID của folder trên Google Drive để lưu file
        /// Lấy từ URL: https://drive.google.com/drive/folders/{FOLDER_ID}
        /// </summary>
        public string ContractsFolderId { get; set; } = string.Empty;

        /// <summary>
        /// (Optional) Số ngày link có hiệu lực mặc định
        /// null = vĩnh viễn
        /// </summary>
        public int? DefaultExpirationDays { get; set; } = null;

        /// <summary>
        /// (Optional) Có tự động xóa file local sau khi upload không
        /// </summary>
        public bool DeleteLocalFileAfterUpload { get; set; } = true;
    }
}