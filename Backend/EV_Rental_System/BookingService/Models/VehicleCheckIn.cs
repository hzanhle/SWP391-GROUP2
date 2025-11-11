using System.ComponentModel.DataAnnotations;

namespace BookingService.Models
{
    /// <summary>
    /// Thông tin xác nhận nhận xe khi bắt đầu thuê
    /// </summary>
    public class VehicleCheckIn
    {
        [Key]
        public int CheckInId { get; set; }

        /// <summary>
        /// Đơn hàng liên quan
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Thời gian nhận xe thực tế
        /// </summary>
        public DateTime CheckInTime { get; set; }

        /// <summary>
        /// Số km hiện tại của xe (nếu có)
        /// </summary>
        public int? OdometerReading { get; set; }

        /// <summary>
        /// Mức nhiên liệu/pin hiện tại (%)
        /// </summary>
        public int? FuelLevel { get; set; }

        /// <summary>
        /// URL các ảnh xe trước khi cho thuê (phân cách bằng dấu ;)
        /// Ví dụ: "front.jpg;back.jpg;left.jpg;right.jpg;dashboard.jpg"
        /// </summary>
        [MaxLength(2000)]
        public string ImageUrls { get; set; } = string.Empty;

        /// <summary>
        /// Ghi chú về tình trạng xe (nếu có)
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }

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
