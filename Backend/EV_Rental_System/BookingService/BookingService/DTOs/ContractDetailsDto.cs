// Đặt ở một thư mục DTOs, ví dụ: BookingService.ViewModels or BookingService.Dtos

/// <summary>
/// Trạng thái TỔNG HỢP của hợp đồng (được TÍNH TOÁN bởi Service).
/// </summary>
namespace BookingService.DTOs
{
    public enum ContractStatus
    {
        Draft,      // Chờ thanh toán (Contract.Active && Payment.Pending)
        Signed,     // Đã ký (Payment.Completed)
        Expired,    // Hết hạn (Contract.Expired)
        Cancelled,  // Đã hủy (Contract.Cancelled)
        Failed      // Thanh toán thất bại (Payment.Failed)
    }

    /// <summary>
    /// DTO trả về thông tin chi tiết hợp đồng cho Frontend.
    /// </summary>
    public class ContractDetailsDto
    {
        public int ContractId { get; set; }
        public int OrderId { get; set; }
        public string ContractNumber { get; set; }
        public string DownloadUrl { get; set; }
        public ContractStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public string TransactionId { get; set; }
        public string Message { get; set; }
    }
}