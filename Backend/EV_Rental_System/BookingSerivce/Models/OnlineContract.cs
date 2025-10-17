namespace BookingService.Models
{
    public class OnlineContract
    {
        public int OnlineContractId { get; set; }

        // Foreign key
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        // Contract identification
        public string ContractNumber { get; set; } = string.Empty;  // CT-2024-00001

        // File storage
        public string ContractFilePath { get; set; } = string.Empty; // "/contracts/CT-2024-00001.pdf"

        // Contract status
        // "Draft" - Vừa tạo, chưa thanh toán
        // "Signed" - Đã thanh toán = tự động ký
        // "Expired" - Hết hạn (không thanh toán trong thời gian quy định)
        // "Cancelled" - Đã hủy
        public string Status { get; set; } = "Draft";

        // Signature info (ký bằng thanh toán)
        public DateTime? SignedAt { get; set; }
        public string? SignatureData { get; set; }         // Payment TransactionId

        // Expiration (deadline thanh toán)
        public DateTime? ExpiresAt { get; set; }

        // Template version (để track nếu thay đổi điều khoản)
        public int TemplateVersion { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Constructors
        public OnlineContract() { }

        public OnlineContract(int orderId, string contractNumber, string filePath, DateTime orderFromDate)
        {
            OrderId = orderId;
            ContractNumber = contractNumber;
            ContractFilePath = filePath;
            Status = "Draft";
            CreatedAt = DateTime.UtcNow;

            // Tính ExpiresAt thông minh
            ExpiresAt = CalculateExpirationDate(orderFromDate);
        }

        // Helper methods
        private DateTime CalculateExpirationDate(DateTime orderFromDate)
        {
            var now = DateTime.UtcNow;
            var daysUntilRental = (orderFromDate.Date - now.Date).Days;

            // Last minute booking (thuê trong vòng 24h)
            if (daysUntilRental <= 1)
            {
                return now.AddHours(2);  // 2 giờ để thanh toán
            }

            // Booking gần (2-7 ngày)
            if (daysUntilRental < 7)
            {
                return orderFromDate.AddDays(-1);  // Phải thanh toán trước 1 ngày
            }

            // Booking xa (>7 ngày)
            return now.AddDays(7);  // 7 ngày để thanh toán
        }

        public void SignViaPayment(string transactionId, string ipAddress)
        {
            if (Status == "Signed")
            {
                throw new InvalidOperationException("Contract already signed");
            }

            if (Status == "Expired" || Status == "Cancelled")
            {
                throw new InvalidOperationException($"Cannot sign contract with status: {Status}");
            }

            if (ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value)
            {
                Status = "Expired";
                throw new InvalidOperationException("Contract has expired");
            }

            Status = "Signed";
            SignedAt = DateTime.UtcNow;
            SignatureData = transactionId;  // Transaction ID = chữ ký
            UpdatedAt = DateTime.UtcNow;
        }

        public bool IsExpired()
        {
            return ExpiresAt.HasValue &&
                   DateTime.UtcNow > ExpiresAt.Value &&
                   Status != "Signed";
        }

        public void MarkAsExpired()
        {
            if (IsExpired())
            {
                Status = "Expired";
                UpdatedAt = DateTime.UtcNow;
            }
        }

        public void Cancel()
        {
            if (Status == "Signed")
            {
                throw new InvalidOperationException("Cannot cancel signed contract");
            }

            Status = "Cancelled";
            UpdatedAt = DateTime.UtcNow;
        }

        public TimeSpan? GetTimeUntilExpiration()
        {
            if (!ExpiresAt.HasValue || Status == "Signed")
            {
                return null;
            }

            var remaining = ExpiresAt.Value - DateTime.UtcNow;
            return remaining.TotalSeconds > 0 ? remaining : TimeSpan.Zero;
        }
    }
}