using BookingService.Models;

namespace BookingService.Models
{
    public class OnlineContract
    {
        public int ContractId { get; set; }

        // Foreign key đến Order - quan hệ 1-1
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        // Nội dung điều khoản hợp đồng (có thể lưu dưới dạng HTML, JSON, hoặc plain text)
        public string Terms { get; set; } = string.Empty;

        // Số hợp đồng - unique identifier cho hợp đồng (VD: CT-2024-00001)
        public string ContractNumber { get; set; } = string.Empty;

        // Trạng thái hợp đồng
        // "Draft" - Bản nháp
        // "Sent" - Đã gửi cho khách
        // "Signed" - Đã ký
        // "Expired" - Hết hạn (nếu không ký trong thời gian quy định)
        // "Cancelled" - Đã hủy
        public string Status { get; set; } = "Draft";

        public DateTime CreatedAt { get; set; }

        // Thời điểm khách hàng ký hợp đồng
        public DateTime? SignedAt { get; set; }

        // Thời điểm hợp đồng hết hạn (nếu không ký trong X ngày)
        public DateTime? ExpiresAt { get; set; }

        // Chữ ký điện tử hoặc confirmation code
        public string? SignatureData { get; set; }

        // IP address của người ký (để có bằng chứng pháp lý)
        public string? SignedFromIpAddress { get; set; }

        // File PDF của hợp đồng (nếu có)
        public string? PdfFilePath { get; set; }

        // Version của template hợp đồng (để track thay đổi terms theo thời gian)
        public int TemplateVersion { get; set; } = 1;

        public DateTime? UpdatedAt { get; set; }

        public OnlineContract()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public OnlineContract(int orderId, string terms, string contractNumber, int templateVersion)
        {
            OrderId = orderId;
            Terms = terms;
            ContractNumber = contractNumber;
            TemplateVersion = templateVersion;
            CreatedAt = DateTime.UtcNow;
            Status = "Draft";
            // Set expiration date (ví dụ: 7 ngày từ khi tạo)
            ExpiresAt = DateTime.UtcNow.AddDays(7);
        }

        // Method để ký hợp đồng
        public void SignContract(string signatureData, string ipAddress)
        {
            SignedAt = DateTime.UtcNow;
            SignatureData = signatureData;
            SignedFromIpAddress = ipAddress;
            Status = "Signed";
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
