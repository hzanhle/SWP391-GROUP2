namespace BookingService.Models
{
    /// <summary>
    /// Lưu thông tin hợp đồng đã được tạo sau khi thanh toán thành công.
    /// </summary>
    public class OnlineContract
    {
        public int OnlineContractId { get; set; }

        // === Foreign Key đến Order ===
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        // === Contract identification ===
        public string ContractNumber { get; set; } = string.Empty;

        // === File info ===
        public string ContractFilePath { get; set; } = string.Empty;

        // === Signature info ===
        public DateTime SignedAt { get; set; } // Thời điểm ký hợp đồng là lúc thanh toán thành công
        public string SignatureData { get; set; } = string.Empty; // TransactionId từ Payment

        public int TemplateVersion { get; set; } = 1;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public OnlineContract() { }

        public OnlineContract(string contractNumber, string filePath, DateTime signedAt, string signatureData)
        {
            ContractNumber = contractNumber;
            ContractFilePath = filePath;
            SignedAt = signedAt;
            SignatureData = signatureData;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
