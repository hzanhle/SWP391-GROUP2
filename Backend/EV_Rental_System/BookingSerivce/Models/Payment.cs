using System.Text.Json.Serialization;

namespace BookingService.Models
{
    public enum PaymentStatus
    {
        Pending,      // Chờ thanh toán
        Completed,    // Thanh toán thành công
        Failed,       // Thanh toán thất bại
        Refunded      // Đã hoàn tiền (nếu cancel)
    }

    public class Payment
    {
        public int PaymentId { get; set; }

        // Foreign key
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        // Payment details
        public decimal Amount { get; set; }            // Tổng tiền phải trả (Deposit + ServiceFee nếu new user)
        public string PaymentMethod { get; set; } = string.Empty; // VNPay, Momo, Cash, etc.

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        // Transaction info từ payment gateway
        public string? TransactionId { get; set; }     // Mã giao dịch từ gateway
        public DateTime? PaidAt { get; set; }          // Thời điểm thanh toán thành công

        // Metadata
        public string? PaymentGatewayResponse { get; set; }  // Raw response từ gateway (JSON)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Constructors
        public Payment() { }

        public Payment(int orderId, decimal amount, string paymentMethod)
        {
            OrderId = orderId;
            Amount = amount;
            PaymentMethod = paymentMethod;
            Status = PaymentStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }

        // Methods
        public void MarkAsCompleted(string transactionId, string? gatewayResponse = null)
        {
            if (Status == PaymentStatus.Completed)
            {
                throw new InvalidOperationException("Payment already completed");
            }

            Status = PaymentStatus.Completed;
            TransactionId = transactionId;
            PaidAt = DateTime.UtcNow;
            PaymentGatewayResponse = gatewayResponse;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsFailed(string? gatewayResponse = null)
        {
            Status = PaymentStatus.Failed;
            PaymentGatewayResponse = gatewayResponse;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool IsCompleted()
        {
            return Status == PaymentStatus.Completed;
        }
    }
}