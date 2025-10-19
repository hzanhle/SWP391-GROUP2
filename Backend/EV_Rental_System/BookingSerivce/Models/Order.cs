using System.Text.Json.Serialization;
namespace BookingService.Models
{
    public enum OrderStatus
    {
        Pending,   // Vừa tạo, chờ thanh toán
        Confirmed, // Đã thanh toán, hợp đồng đã tạo
        InProgress, // Đang trong chuyến đi
        Completed, // Đã hoàn thành chuyến đi
        Cancelled  // Đã hủy (bởi user hoặc do hết hạn)
    }

    public class Order
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public int VehicleId { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public decimal HourlyRate { get; set; }
        public decimal TotalCost { get; set; }
        public decimal DepositAmount { get; set; }
        public int InitialTrustScore { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        /// <summary>
        /// (MỚI) Hạn chót để thanh toán cho Order này.
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Constructors (Giữ nguyên)
        public Order() { }

        public Order(int userId, int vehicleId, DateTime fromDate, DateTime toDate,
                     decimal hourlyRate, decimal totalCost, decimal depositAmount, int trustScore)
        {
            UserId = userId;
            VehicleId = vehicleId;
            FromDate = fromDate;
            ToDate = toDate;
            HourlyRate = hourlyRate;
            TotalCost = totalCost;
            DepositAmount = depositAmount;
            InitialTrustScore = trustScore;
            Status = OrderStatus.Pending;
            CreatedAt = DateTime.UtcNow;
            // ExpiresAt sẽ được set bởi Service
        }

        // --- Cập nhật Methods ---

        public void Confirm()
        {
            if (Status != OrderStatus.Pending)
                throw new InvalidOperationException("Only Pending orders can be confirmed.");
            Status = OrderStatus.Confirmed;
            ExpiresAt = null; // (QUAN TRỌNG) Không còn hết hạn nữa
        }

        public void Cancel()
        {
            if (Status == OrderStatus.Completed || Status == OrderStatus.InProgress)
                throw new InvalidOperationException($"Cannot cancel order with status: {Status}");
            Status = OrderStatus.Cancelled;
            ExpiresAt = null; // (QUAN TRỌNG) Không còn hết hạn nữa
        }

        public void StartRental()
        {
            if (Status != OrderStatus.Confirmed)
                throw new InvalidOperationException("Only Confirmed orders can be started.");
            Status = OrderStatus.InProgress;
        }

        public void Complete()
        {
            if (Status != OrderStatus.InProgress)
                throw new InvalidOperationException("Only InProgress orders can be completed.");
            Status = OrderStatus.Completed;
        }
    }
}