namespace BookingSerivce.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }

        // Foreign key đến Order - quan hệ 1-1
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        // Ngày thanh toán đặt cọc
        public DateTime? DepositDate { get; set; }

        // Ngày thanh toán toàn bộ
        public DateTime? FullPaymentDate { get; set; }

        // Phương thức thanh toán
        // "CreditCard", "DebitCard", "BankTransfer", "Cash", "EWallet"
        public string PaymentMethod { get; set; } = string.Empty;

        // Trạng thái đặt cọc
        public bool IsDeposited { get; set; } = false;

        // Số tiền đã đặt cọc
        public decimal DepositedAmount { get; set; }

        // Trạng thái thanh toán đầy đủ
        public bool IsFullyPaid { get; set; } = false;

        // Số tiền đã thanh toán (có thể khác TotalCost nếu có discount/refund)
        public decimal PaidAmount { get; set; }

        // Trạng thái thanh toán tổng thể
        // "Pending" - Chờ thanh toán
        // "DepositPaid" - Đã đặt cọc
        // "FullyPaid" - Đã thanh toán đầy đủ
        // "Refunded" - Đã hoàn tiền
        // "PartialRefund" - Hoàn tiền một phần
        public string Status { get; set; } = "Pending";

        // Mã giao dịch từ payment gateway
        public string? TransactionCode { get; set; }

        // Mã giao dịch đặt cọc (nếu khác với thanh toán chính)
        public string? DepositTransactionCode { get; set; }

        // Ghi chú về thanh toán (lý do refund, discount, etc.)
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Payment()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public Payment(int orderId, string paymentMethod)
        {
            OrderId = orderId;
            PaymentMethod = paymentMethod;
            CreatedAt = DateTime.UtcNow;
            Status = "Pending";
        }
    }
}
