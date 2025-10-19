namespace BookingService.Models
{
    /// <summary>
    /// Model này chỉ lưu thông tin về hợp đồng ĐÃ ĐƯỢC TẠO
    /// (tức là sau khi thanh toán thành công).
    /// Nó không còn quản lý trạng thái "Draft" hay "Expired".
    /// </summary>
    public class OnlineContract
    {
        public int OnlineContractId { get; set; }

        // Foreign key
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        // Contract identification
        public string ContractNumber { get; set; } = string.Empty;

        // File storage
        public string ContractFilePath { get; set; } = string.Empty;

        // Thông tin "ký" (lấy từ Payment khi tạo)
        public DateTime SignedAt { get; set; }
        public string SignatureData { get; set; } // Payment TransactionId

        // Template version
        public int TemplateVersion { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Constructors
        public OnlineContract() { }

        public OnlineContract(int orderId, string contractNumber, string filePath, DateTime signedAt, string signatureData)
        {
            OrderId = orderId;
            ContractNumber = contractNumber;
            ContractFilePath = filePath;
            SignedAt = signedAt;
            SignatureData = signatureData;
            CreatedAt = DateTime.UtcNow;
        }
    }
}