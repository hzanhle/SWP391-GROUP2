namespace BookingSerivce.DTOs
{
    public class NotificationRequest
    {
        public int? Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string DataType { get; set; } // Loại dữ liệu liên quan (ví dụ: "Order", "Message", v.v.)
        public int? DataId { get; set; } // ID liên quan đến dữ liệu cụ thể (nếu có)
        public int UserId { get; set; }
        public int? StaffId { get; set; }

    }
}
