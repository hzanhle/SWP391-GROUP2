namespace BookingService.DTOs
{
    /// <summary>
    /// DTO nhận từ FE chứa toàn bộ dữ liệu để tạo hợp đồng
    /// </summary>
    public class ContractDataDto
    {
        // ===== Thông tin hợp đồng =====
        public int OrderId { get; set; }
        public string ContractNumber { get; set; } // Backend tạo, ví dụ: "CONTRACT-20251020-001"
        public DateTime? PaidAt { get; set; }      // Thời gian thanh toán

        // ===== Thông tin bên thuê (Khách hàng) =====
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string? CustomerIdCard { get; set; } // ✅ Optional
        public string CustomerAddress { get; set; }
        public string CustomerDateOfBirth { get; set; } // Format: DD-MM-YYYY

        // ===== Thông tin bên cho thuê (Công ty) - Có giá trị mặc định =====
        public string CompanyName { get; set; } = "Công ty TNHH Cho Thuê Xe XYZ";
        public string CompanyAddress { get; set; } = "123 Đường ABC, TP. Hồ Chí Minh";
        public string CompanyTaxCode { get; set; } = "0123456789";
        public string CompanyRepresentative { get; set; } = "Ông Nguyễn Văn A";

        // ===== Thông tin xe =====
        public string VehicleModel { get; set; }   // Ví dụ: "VinFast VF8"
        public string LicensePlate { get; set; }   // Ví dụ: "51K-123.45"
        public string VehicleColor { get; set; }   // Ví dụ: "Trắng"
        public string VehicleType { get; set; }    // Ví dụ: "SUV"

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
        public string PaymentMethod { get; set; }  // Ví dụ: "VNPay"
        public DateTime PaymentDate { get; set; }
    }
}
