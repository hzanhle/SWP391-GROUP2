namespace BookingService.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }

        // Foreign key đến Order - quan hệ 1-1
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        // Phương thức thanh toán (VNPay, Momo, BankTransfer, Cash, etc.)
        public string PaymentMethod { get; set; } = string.Empty;

        // Trạng thái thanh toán tổng thể
        // "Pending" - Chờ thanh toán
        // "PartiallyPaid" - Đã cọc
        // "FullyPaid" - Đã thanh toán đủ
        // "Refunded" - Đã hoàn tiền
        // "Failed" - Thanh toán thất bại
        public string Status { get; set; } = "Pending";

        // Thông tin cọc (Deposit)
        public bool IsDeposited { get; set; } = false;
        public decimal DepositedAmount { get; set; } = 0;
        public DateTime? DepositDate { get; set; }
        public string? DepositTransactionCode { get; set; }

        // Thông tin thanh toán đầy đủ (Full Payment)
        public bool IsFullyPaid { get; set; } = false;
        public decimal PaidAmount { get; set; } = 0; // Tổng số tiền đã thanh toán
        public DateTime? FullPaymentDate { get; set; }
        public string? TransactionCode { get; set; }

        // Thông tin bổ sung
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Stage 2 Enhancement Fields - Single-step payment (deposit only)
        public decimal? Amount { get; set; } // Amount for this payment (deposit amount)
        public string? Method { get; set; } // Payment method used
        public string? TransactionId { get; set; } // Transaction ID from payment gateway
        public DateTime? CompletedAt { get; set; } // When payment was completed

        // Constructors
        public Payment()
        {
            CreatedAt = DateTime.UtcNow;
            Status = "Pending";
            IsDeposited = false;
            IsFullyPaid = false;
            DepositedAmount = 0;
            PaidAmount = 0;
        }

        public Payment(int orderId, string paymentMethod)
        {
            OrderId = orderId;
            PaymentMethod = paymentMethod;
            Status = "Pending";
            IsDeposited = false;
            IsFullyPaid = false;
            DepositedAmount = 0;
            PaidAmount = 0;
            CreatedAt = DateTime.UtcNow;
        }

        public Payment(int orderId, string paymentMethod, string notes)
        {
            OrderId = orderId;
            PaymentMethod = paymentMethod;
            Notes = notes;
            Status = "Pending";
            IsDeposited = false;
            IsFullyPaid = false;
            DepositedAmount = 0;
            PaidAmount = 0;
            CreatedAt = DateTime.UtcNow;
        }

        // Methods
        public void RecordDeposit(decimal amount, string transactionCode)
        {
            if (IsDeposited)
            {
                throw new InvalidOperationException("Deposit has already been recorded");
            }

            DepositedAmount = amount;
            DepositDate = DateTime.UtcNow;
            DepositTransactionCode = transactionCode;
            IsDeposited = true;
            PaidAmount += amount;
            Status = "PartiallyPaid";
            UpdatedAt = DateTime.UtcNow;
        }

        public void RecordFullPayment(decimal amount, string transactionCode)
        {
            if (IsFullyPaid)
            {
                throw new InvalidOperationException("Payment has already been completed");
            }

            PaidAmount += amount;
            FullPaymentDate = DateTime.UtcNow;
            TransactionCode = transactionCode;
            IsFullyPaid = true;
            Status = "FullyPaid";
            UpdatedAt = DateTime.UtcNow;
        }

        public decimal GetRemainingAmount(decimal totalCost)
        {
            return totalCost - PaidAmount;
        }
    }
}