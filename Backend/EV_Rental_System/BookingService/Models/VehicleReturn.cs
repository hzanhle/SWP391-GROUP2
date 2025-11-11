using System.ComponentModel.DataAnnotations;

namespace BookingService.Models
{
    /// <summary>
    /// Thông tin xác nhận trả xe khi kết thúc thuê
    /// </summary>
    public class VehicleReturn
    {
        [Key]
        public int ReturnId { get; set; }

        /// <summary>
        /// Đơn hàng liên quan
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Thời gian trả xe thực tế
        /// </summary>
        public DateTime ReturnTime { get; set; }

        /// <summary>
        /// Số km khi trả xe (nếu có)
        /// </summary>
        public int? OdometerReading { get; set; }

        /// <summary>
        /// Mức nhiên liệu/pin khi trả xe (%)
        /// </summary>
        public int? FuelLevel { get; set; }

        /// <summary>
        /// URL các ảnh xe khi trả lại (phân cách bằng dấu ;)
        /// Ví dụ: "front.jpg;back.jpg;left.jpg;right.jpg;dashboard.jpg"
        /// </summary>
        [MaxLength(2000)]
        public string ImageUrls { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả tình trạng xe khi trả
        /// </summary>
        [MaxLength(1000)]
        public string? ConditionNotes { get; set; }

        /// <summary>
        /// Có hư hỏng không?
        /// </summary>
        public bool HasDamage { get; set; }

        /// <summary>
        /// Mô tả chi tiết hư hỏng (nếu có)
        /// </summary>
        [MaxLength(2000)]
        public string? DamageDescription { get; set; }

        /// <summary>
        /// Phí bồi thường hư hỏng (nếu có)
        /// </summary>
        public decimal DamageCharge { get; set; }

        /// <summary>
        /// Người xác nhận (Employee ID hoặc Member ID)
        /// </summary>
        public int ConfirmedBy { get; set; }

        /// <summary>
        /// Thời gian tạo bản ghi
        /// </summary>
        public DateTime CreatedAt { get; set; }

        // Navigation property
        public Order? Order { get; set; }
    }
}
