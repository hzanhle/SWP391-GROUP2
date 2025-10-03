namespace BookingSerivce.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        public int UserId { get; set; }
        // Navigation property đến User (nên thêm nếu có User model)
        // public User? User { get; set; }

        public int StaffId { get; set; } // Nhân viên xử lý đơn

        public int VehicleId { get; set; }
        // Navigation property đến Vehicle (nên thêm nếu có Vehicle model)
        // public Vehicle? Vehicle { get; set; }

        // Thông tin thời gian thuê - di chuyển từ OrderDetail vì đây là thông tin cốt lõi của Order
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        // Tổng số ngày thuê - tính toán từ FromDate và ToDate
        public int TotalDays { get; set; }

        // Tổng chi phí - có thể tính toán hoặc lưu trữ
        public decimal TotalCost { get; set; }

        // Tiền đặt cọc (deposit) - thường là % của TotalCost
        public decimal DepositAmount { get; set; }

        // Trạng thái đơn hàng tổng thể
        // "Pending" - Chờ xác nhận
        // "Confirmed" - Đã xác nhận
        // "InProgress" - Đang thuê
        // "Completed" - Hoàn thành
        // "Cancelled" - Đã hủy
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; } // Thêm để track thời điểm cập nhật

        // One-to-One relationships
        public Payment? Payment { get; set; }
        public OnlineContract? OnlineContract { get; set; }

        public Order()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public Order(int userId, int vehicleId, DateTime fromDate, DateTime toDate,
                     decimal totalCost, decimal depositAmount)
        {
            UserId = userId;
            VehicleId = vehicleId;
            FromDate = fromDate;
            ToDate = toDate;
            TotalDays = (toDate - fromDate).Days;
            TotalCost = totalCost;
            DepositAmount = depositAmount;
            CreatedAt = DateTime.UtcNow;
            Status = "Pending";
        }
    }
}
