using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BookingService.Models
{
    public enum PaymentStatus
    {
        Pending,      // Chờ thanh toán
        Completed,    // Thanh toán thành công
        Failed,       // Thanh toán thất bại
        Cancelled,    // Đã hủy (do timeout hoặc user cancel)
        Refunded      // Đã hoàn tiền (nếu cancel sau khi completed)
    }

    public class Payment
    {
        public int PaymentId { get; set; }

        // === Foreign Key đến Order ===
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        // === Payment details ===
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty; // VNPay, Momo, Cash, etc.

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        // === Transaction info ===
        public string? TransactionId { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? PaymentGatewayResponse { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Thời điểm hết hạn thanh toán (để background job hủy nếu quá thời gian)
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        // === Helper Properties ===
        [NotMapped]
        public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;

        [NotMapped]
        public TimeSpan? TimeRemaining => ExpiresAt.HasValue && !IsExpired
            ? ExpiresAt.Value - DateTime.UtcNow
            : null;

        // === Constructors ===
        public Payment() { }

        // Constructor chính - Nhận đầy đủ thông tin cần thiết
        public Payment(int orderId, decimal amount, string paymentMethod, int expirationMinutes = 15)
        {
            if (orderId <= 0)
                throw new ArgumentException("OrderId must be greater than 0", nameof(orderId));
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than 0", nameof(amount));
            if (string.IsNullOrWhiteSpace(paymentMethod))
                throw new ArgumentException("PaymentMethod cannot be empty", nameof(paymentMethod));

            OrderId = orderId;
            Amount = amount;
            PaymentMethod = paymentMethod;
            Status = PaymentStatus.Pending;
            CreatedAt = DateTime.UtcNow;
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);
        }

        // === Domain Methods ===
        public void MarkAsCompleted(string transactionId, string? gatewayResponse = null)
        {
            if (Status == PaymentStatus.Completed)
                throw new InvalidOperationException($"Payment {PaymentId} is already completed.");
            if (string.IsNullOrWhiteSpace(transactionId))
                throw new ArgumentException("TransactionId cannot be empty", nameof(transactionId));

            Status = PaymentStatus.Completed;
            TransactionId = transactionId;
            PaidAt = DateTime.UtcNow;
            PaymentGatewayResponse = gatewayResponse;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsFailed(string? gatewayResponse = null)
        {
            if (Status == PaymentStatus.Completed)
                throw new InvalidOperationException($"Cannot mark completed payment {PaymentId} as failed.");

            Status = PaymentStatus.Failed;
            PaymentGatewayResponse = gatewayResponse;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsCancelled(string? reason = null)
        {
            if (Status == PaymentStatus.Completed)
                throw new InvalidOperationException($"Cannot cancel completed payment {PaymentId}.");
            if (Status == PaymentStatus.Cancelled)
                throw new InvalidOperationException($"Payment {PaymentId} is already cancelled.");

            Status = PaymentStatus.Cancelled;
            PaymentGatewayResponse = reason;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsRefunded(string? reason = null)
        {
            if (Status != PaymentStatus.Completed)
                throw new InvalidOperationException($"Cannot refund payment {PaymentId} that is not completed.");

            Status = PaymentStatus.Refunded;
            PaymentGatewayResponse = reason;
            UpdatedAt = DateTime.UtcNow;
        }

        // === Query Methods ===
        public bool IsCompleted() => Status == PaymentStatus.Completed;
        public bool IsPending() => Status == PaymentStatus.Pending;
        public bool IsFailed() => Status == PaymentStatus.Failed;
        public bool IsCancelled() => Status == PaymentStatus.Cancelled;
        public bool IsRefunded() => Status == PaymentStatus.Refunded;
        public bool CanBeRefunded() => Status == PaymentStatus.Completed;
        public bool CanBeCancelled() => Status == PaymentStatus.Pending;
    }
}