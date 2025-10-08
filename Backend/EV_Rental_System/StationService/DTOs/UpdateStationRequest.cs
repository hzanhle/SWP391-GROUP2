namespace StationService.DTOs
{
    public class UpdateStationRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public int? ManagerId { get; set; } // User với role là Staff
        public bool IsActive { get; set; } // Dùng để kích hoạt/vô hiệu hóa trạng thái hoạt động của trạm

    }
}
