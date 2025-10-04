using System.ComponentModel.DataAnnotations;

namespace BookingService.Models
{
    // Model lưu hình ảnh kiểm tra - CHỈ lưu đường dẫn file
    public class InspectionImage
    {
        [Key]
        public int ImageId { get; set; }

        // Đường dẫn file ảnh (VD: "uploads/inspections/2024/10/abc123.jpg")
        public string ImagePath { get; set; } = string.Empty;

        // Loại ảnh: "Overview", "Damage", "Detail", "Mileage", "Fuel"
        public string ImageType { get; set; } = "Detail";

        // Foreign key đến VehicleInspectionReport
        public int InspectionId { get; set; }

        // Navigation property
        public VehicleInspectionReport? Inspection { get; set; }

        // Foreign key đến InspectionDetail (optional)
        public int? DetailId { get; set; }

        // Navigation property
        public InspectionDetail? Detail { get; set; }

        public InspectionImage() { }

        public InspectionImage(int inspectionId, string imagePath, string imageType = "Detail")
        {
            InspectionId = inspectionId;
            ImagePath = imagePath;
            ImageType = imageType;
        }

        public InspectionImage(int inspectionId, int? detailId, string imagePath, string imageType = "Detail")
        {
            InspectionId = inspectionId;
            DetailId = detailId;
            ImagePath = imagePath;
            ImageType = imageType;
        }
    }
}