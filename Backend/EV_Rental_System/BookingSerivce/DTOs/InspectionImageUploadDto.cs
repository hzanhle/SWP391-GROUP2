namespace BookingSerivce.DTOs
{
    // DTO để nhận file upload từ client (dùng trong Controller/API)
    public class InspectionImageUploadDto
    {
        public int InspectionId { get; set; }
        public int? DetailId { get; set; } // Optional
        public string ImageType { get; set; } = "Detail";
        public string? Description { get; set; }
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
    }
}
