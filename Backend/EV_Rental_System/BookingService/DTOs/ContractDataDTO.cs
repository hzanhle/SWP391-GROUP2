namespace BookingService.DTOs
{
    /// <summary>
    /// DTO nhận từ FE chứa toàn bộ dữ liệu để tạo hợp đồng
    /// </summary>
    public class ContractDataDto
    {
        // ===== Thông tin hợp đồng =====
        public int OrderId { get; set; }
        public string? ContractNumber { get; set; } // ✅ Optional - Backend tạo, ví dụ: "CONTRACT-20251020-001"
        public DateTime? PaidAt { get; set; }       // Thời gian thanh toán

        // ===== Thông tin bên thuê (Khách hàng) =====
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string? CustomerIdCard { get; set; } // ✅ Optional
        public string CustomerAddress { get; set; } = string.Empty;
        public string CustomerDateOfBirth { get; set; } = string.Empty;

        // ===== Thông tin bên cho thuê (Công ty) - Có giá trị mặc định =====
        public string CompanyName { get; set; } = "Công ty TNHH Cho Thuê Xe XYZ";
        public string CompanyAddress { get; set; } = "123 Đường ABC, TP. Hồ Chí Minh";
        public string CompanyTaxCode { get; set; } = "0123456789";
        public string CompanyRepresentative { get; set; } = "Ông Nguyễn Văn A";

        // ===== Thông tin xe =====
        public string VehicleModel { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string VehicleColor { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;

        // ===== Thông tin thuê =====
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        // ===== Thông tin tài chính =====
        public decimal TotalRentalCost { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal ServiceFee { get; set; }
        public decimal TotalPaymentAmount { get; set; }

        // ===== Thông tin thanh toán =====
        public string? TransactionId { get; set; }  // ✅ Optional - ID giao dịch
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
    }
}
