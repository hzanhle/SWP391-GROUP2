namespace BookingService.Models
{
    // Model chi tiết kiểm tra - lưu từng hạng mục kiểm tra cụ thể
    public class InspectionDetail
    {
        public int DetailId { get; set; }

        // Foreign key đến VehicleInspectionReport
        public int InspectionId { get; set; }
        public VehicleInspectionReport? Inspection { get; set; }

        // Danh mục kiểm tra (category)
        // "Exterior" - Ngoại thất
        // "Interior" - Nội thất
        // "Engine" - Động cơ
        // "Tires" - Lốp xe
        // "Electronics" - Điện tử
        // "Documents" - Giấy tờ
        public string Category { get; set; } = string.Empty;

        // Hạng mục cụ thể
        // VD: "Front Bumper", "Windshield", "Seat", "Tire - Front Left", etc.
        public string ItemName { get; set; } = string.Empty;

        // Có vấn đề không
        public bool HasIssue { get; set; } = false;

        // Mức độ nghiêm trọng
        // "Minor" - Nhỏ (vết xước nhẹ)
        // "Moderate" - Trung bình (móp vỡ nhỏ)
        // "Major" - Nghiêm trọng (hư hỏng lớn)
        // "Critical" - Nguy hiểm (ảnh hưởng an toàn)
        public string? Severity { get; set; }

        // Mô tả chi tiết vấn đề
        public string? IssueDescription { get; set; }

        // Yêu cầu đền bù
        public bool RequiresCompensation { get; set; } = false;

        // Số tiền đền bù cho hạng mục này
        public decimal CompensationAmount { get; set; } = 0;

        // Vị trí cụ thể (VD: "Front Left", "Rear Right", etc.)
        public string? Location { get; set; }

        // Trạng thái
        // "OK" - Bình thường
        // "Damaged" - Hư hỏng
        // "Missing" - Thiếu
        // "NeedsAttention" - Cần chú ý
        public string Status { get; set; } = "OK";

        public DateTime CreatedAt { get; set; }

        public InspectionDetail()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public InspectionDetail(int inspectionId, string category, string itemName)
        {
            InspectionId = inspectionId;
            Category = category;
            ItemName = itemName;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
