using System.Text.Json.Serialization;
namespace BookingService.Models
{
    public enum OrderStatus
    {
        Pending,      // Chờ thanh toán
        Confirmed,    // Đã thanh toán & ký hợp đồng
        InProgress,   // Đang thuê
        Completed,    // Hoàn tất
        Cancelled,    // Đã hủy
        NoShow        // Không đến nhận xe
    }

    public class Order
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public int VehicleId { get; set; }

        // Relationships
        public Payment? Payment { get; set; }
        public OnlineContract? OnlineContract { get; set; }

        // Booking period
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        // Pricing (snapshot tại thời điểm đặt)
        public decimal HourlyRate { get; set; }        // Giá thuê mỗi giờ
        public decimal TotalCost { get; set; }         // Tổng tiền thuê = HourlyRate * Hours
        public decimal DepositAmount { get; set; }     // Tiền cọc

        // User trust score tại thời điểm đặt (để audit)
        public int UserTrustScoreAtBooking { get; set; }

        // Status
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Constructors
        public Order() { }

        public Order(
            int userId,
            int vehicleId,
            DateTime fromDate,
            DateTime toDate,
            decimal hourlyRate,
            decimal totalCost,
            decimal depositAmount,
            int userTrustScore)
        {
            UserId = userId;
            VehicleId = vehicleId;
            FromDate = fromDate;
            ToDate = toDate;
            HourlyRate = hourlyRate;
            TotalCost = totalCost;
            DepositAmount = depositAmount;
            UserTrustScoreAtBooking = userTrustScore;
            Status = OrderStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }

        // ============ STATUS TRANSITION METHODS ============

        public void Confirm()
        {
            if (Status != OrderStatus.Pending)
            {
                throw new InvalidOperationException($"Cannot confirm order with status: {Status}");
            }

            Status = OrderStatus.Confirmed;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Cancel()
        {
            if (Status == OrderStatus.Completed)
            {
                throw new InvalidOperationException("Cannot cancel completed order");
            }

            if (Status == OrderStatus.InProgress)
            {
                throw new InvalidOperationException("Cannot cancel order in progress");
            }

            if (Status == OrderStatus.Cancelled)
            {
                throw new InvalidOperationException("Order already cancelled");
            }

            Status = OrderStatus.Cancelled;
            UpdatedAt = DateTime.UtcNow;
        }

        public void StartRental()
        {
            if (Status != OrderStatus.Confirmed)
            {
                throw new InvalidOperationException($"Cannot start rental for order with status: {Status}");
            }

            if (DateTime.UtcNow < FromDate)
            {
                throw new InvalidOperationException("Cannot start rental before FromDate");
            }

            Status = OrderStatus.InProgress;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Complete()
        {
            if (Status != OrderStatus.InProgress)
            {
                throw new InvalidOperationException($"Cannot complete order with status: {Status}");
            }

            Status = OrderStatus.Completed;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsNoShow()
        {
            if (Status != OrderStatus.Confirmed)
            {
                throw new InvalidOperationException($"Cannot mark as no-show for order with status: {Status}");
            }

            // NoShow chỉ xảy ra khi đã confirmed nhưng user không đến nhận xe
            if (DateTime.UtcNow < FromDate.AddHours(1)) // Grace period 1 giờ
            {
                throw new InvalidOperationException("Too early to mark as no-show");
            }

            Status = OrderStatus.NoShow;
            UpdatedAt = DateTime.UtcNow;
        }

        // ============ HELPER METHODS ============

        public int GetTotalHours()
        {
            return (int)(ToDate - FromDate).TotalHours;
        }

        public bool IsValidDateRange()
        {
            return ToDate > FromDate && FromDate >= DateTime.UtcNow;
        }

        public void UpdateStatus(OrderStatus newStatus)
        {
            Status = newStatus;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool CanBeCancelled()
        {
            return Status == OrderStatus.Pending || Status == OrderStatus.Confirmed;
        }

        public bool IsActive()
        {
            return Status == OrderStatus.Confirmed || Status == OrderStatus.InProgress;
        }

        public bool IsFinalized()
        {
            return Status == OrderStatus.Completed ||
                   Status == OrderStatus.Cancelled ||
                   Status == OrderStatus.NoShow;
        }

        public TimeSpan GetRemainingTime()
        {
            if (Status != OrderStatus.InProgress)
            {
                throw new InvalidOperationException("Order is not in progress");
            }

            var remaining = ToDate - DateTime.UtcNow;
            return remaining.TotalSeconds > 0 ? remaining : TimeSpan.Zero;
        }

        public bool IsOverdue()
        {
            return Status == OrderStatus.InProgress && DateTime.UtcNow > ToDate;
        }
    }
}