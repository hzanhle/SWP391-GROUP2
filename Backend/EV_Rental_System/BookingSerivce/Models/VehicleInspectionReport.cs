namespace BookingService.Models
{
    // Model báo cáo kiểm tra xe - quản lý việc kiểm tra khi nhận và trả xe
    public class VehicleInspectionReport
    {
        public int InspectionId { get; set; }

        // Foreign key đến Order
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        // Loại kiểm tra
        // "PickUp" - Kiểm tra khi khách nhận xe (bàn giao xe cho khách)
        // "Return" - Kiểm tra khi khách trả xe (nhận xe lại từ khách)
        public string InspectionType { get; set; } = string.Empty;

        // Ngày giờ kiểm tra
        public DateTime InspectionDate { get; set; }

        // ID nhân viên thực hiện kiểm tra
        public int InspectorId { get; set; }
        // Navigation property (nên thêm nếu có Staff/User model)
        // public Staff? Inspector { get; set; }

        // Số km hiện tại của xe
        public int CurrentMileage { get; set; }

        // Mức nhiên liệu hiện tại (0-100%)
        public int FuelLevel { get; set; }

        // Tình trạng tổng thể của xe
        // "Excellent" - Xuất sắc
        // "Good" - Tốt
        // "Fair" - Ổn
        // "Poor" - Kém
        // "Damaged" - Có hư hỏng
        public string OverallCondition { get; set; } = "Good";

        // Có hư hỏng không
        public bool HasDamage { get; set; } = false;

        // Tổng số tiền đền bù (nếu có)
        public decimal CompensationAmount { get; set; } = 0;

        // Trạng thái thanh toán đền bù
        // "NotRequired" - Không cần đền bù
        // "Pending" - Chờ thanh toán
        // "Paid" - Đã thanh toán
        // "Waived" - Được miễn
        public string CompensationStatus { get; set; } = "NotRequired";

        // Ghi chú chung về tình trạng xe
        public string? GeneralNotes { get; set; }

        // Chữ ký xác nhận của khách hàng (digital signature hoặc confirmation code)
        public string? CustomerSignature { get; set; }

        // Ngày khách ký xác nhận
        public DateTime? CustomerSignedAt { get; set; }

        // Chữ ký của nhân viên
        public string? InspectorSignature { get; set; }

        // Trạng thái báo cáo
        // "Draft" - Đang soạn
        // "Completed" - Hoàn thành
        // "UnderReview" - Đang xem xét (có tranh chấp)
        // "Approved" - Đã duyệt
        public string Status { get; set; } = "Draft";

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // One-to-many relationship với chi tiết kiểm tra
        public List<InspectionDetail> InspectionDetails { get; set; } = new List<InspectionDetail>();

        // One-to-many relationship với hình ảnh
        public List<InspectionImage> InspectionImages { get; set; } = new List<InspectionImage>();

        public VehicleInspectionReport()
        {
            CreatedAt = DateTime.UtcNow;
            InspectionDate = DateTime.UtcNow;
        }

        public VehicleInspectionReport(int orderId, string inspectionType, int inspectorId,
                                       int currentMileage, int fuelLevel)
        {
            OrderId = orderId;
            InspectionType = inspectionType;
            InspectorId = inspectorId;
            CurrentMileage = currentMileage;
            FuelLevel = fuelLevel;
            CreatedAt = DateTime.UtcNow;
            InspectionDate = DateTime.UtcNow;
        }

        // Method để tính tổng tiền đền bù từ các chi tiết
        public void CalculateTotalCompensation()
        {
            CompensationAmount = InspectionDetails
                .Where(d => d.RequiresCompensation)
                .Sum(d => d.CompensationAmount);

            HasDamage = InspectionDetails.Any(d => d.HasIssue);

            if (CompensationAmount > 0)
            {
                CompensationStatus = "Pending";
            }
        }
    }

    

    

    
}
