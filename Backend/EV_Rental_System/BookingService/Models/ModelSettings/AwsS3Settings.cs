namespace BookingService.Models.ModelSettings
{
    public class AwsS3Settings
    {
        /// <summary>
        /// AWS Access Key ID (từ IAM User)
        /// </summary>
        public string AccessKey { get; set; } = string.Empty;

        /// <summary>
        /// AWS Secret Access Key (từ IAM User)
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>
        /// Tên S3 Bucket (ví dụ: ev-rental-contracts)
        /// </summary>
        public string BucketName { get; set; } = string.Empty;

        /// <summary>
        /// AWS Region (ví dụ: ap-southeast-1, us-east-1)
        /// </summary>
        public string Region { get; set; } = "ap-southeast-1";

        /// <summary>
        /// Thư mục gốc trong bucket (ví dụ: contracts, documents)
        /// </summary>
        public string FolderPath { get; set; } = "contracts";

        /// <summary>
        /// Có tự động tạo public URL không (true = public, false = presigned URL)
        /// </summary>
        public bool EnablePublicAccess { get; set; } = true;

        /// <summary>
        /// Thời gian hết hạn của Presigned URL (phút) - chỉ dùng khi EnablePublicAccess = false
        /// </summary>
        public int PresignedUrlExpirationMinutes { get; set; } = 60;
    }
}