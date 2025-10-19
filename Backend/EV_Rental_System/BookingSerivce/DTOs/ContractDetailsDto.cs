// Đặt ở một thư mục DTOs, ví dụ: BookingService.ViewModels or BookingService.Dtos

/// <summary>
/// Trạng thái TỔNG HỢP của hợp đồng (được TÍNH TOÁN bởi Service).
/// </summary>
public enum OverallContractStatus
{
    Draft,      // Đang chờ thanh toán (Contract.Active && Payment.Pending)
    Signed,     // Đã ký (Payment.Completed)
    Expired,    // Hết hạn (Contract.Expired)
    Cancelled,  // Đã hủy (Contract.Cancelled)
    Failed,     // Thanh toán thất bại (Payment.Failed)
    Refunded    // Đã hoàn tiền (Payment.Refunded)
}

/// <summary>
/// DTO (Data Transfer Object) chứa thông tin chi tiết của hợp đồng
/// để trả về cho API/Client.
/// </summary>
public class ContractDetailsDto
{
    public int ContractId { get; set; }
    public int OrderId { get; set; }
    public string ContractNumber { get; set; }

    /// <summary>
    /// URL an toàn để tải hợp đồng (do Service tạo ra, không lộ đường dẫn vật lý).
    /// </summary>
    public string DownloadUrl { get; set; }

    /// <summary>
    /// Trạng thái tổng hợp đã được TÍNH TOÁN.
    /// </summary>
    public OverallContractStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Deadline để thanh toán.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Thời gian còn lại (được tính toán).
    /// </summary>
    public TimeSpan? TimeRemaining { get; set; }

    // --- Thông tin "Chữ ký" lấy từ Payment ---

    /// <summary>
    /// Ngày ký (Nguồn chân lý: Payment.PaidAt).
    /// </summary>
    public DateTime? SignedAt { get; set; }

    /// <summary>
    /// Dữ liệu chữ ký (Nguồn chân lý: Payment.TransactionId).
    /// </summary>
    public string? SignatureData { get; set; }
}

/// <summary>
/// DTO đầu vào để tạo mới một hợp đồng.
/// </summary>
public class CreateContractRequest
{
    public int OrderId { get; set; }
    // Service sẽ tự lấy OrderFromDate từ OrderRepository
}