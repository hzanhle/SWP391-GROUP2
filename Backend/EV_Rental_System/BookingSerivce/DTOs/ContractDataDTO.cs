namespace BookingService.DTOs
{
    public class ContractDataDTO
    {
        // Thông tin hợp đồng
        public string ContractNumber { get; set; }
        public DateTime SignedAt { get; set; }

        // Thông tin bên thuê (Bên A)
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; } // Lấy từ User
        public string CustomerIdCard { get; set; } // Lấy từ UserProfile

        // Thông tin bên cho thuê (Bên B)
        public string CompanyName { get; set; } = "Công ty TNHH Cho Thuê Xe XYZ";
        public string CompanyAddress { get; set; } = "123 Đường ABC, Phường X, Quận Y, TP. Z";
        public string CompanyTaxCode { get; set; } = "0123456789";
        public string CompanyRepresentative { get; set; } = "Ông Nguyễn Văn A";

        // Thông tin xe
        public string VehicleName { get; set; } // "VinFast VF8"
        public string LicensePlate { get; set; } // "51K-123.45"
        public string VehicleColor { get; set; } // "Trắng"

        // Thông tin thuê
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        // Thông tin tài chính
        public decimal TotalRentalCost { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal ServiceFee { get; set; }
        public decimal TotalPaymentAmount { get; set; }

        // Thông tin chữ ký (thanh toán)
        public string TransactionId { get; set; } // Đây là "chữ ký"
        public string PaymentMethod { get; set; } // "VNPay"
    }
}