namespace BookingService.Models
{
    public class Notification
    {
        public int? Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        // Type của notification: "OrderConfirmed", "PaymentSuccess", "Reminder", etc.
        public string DataType { get; set; }

        // ID liên quan (OrderId, PaymentId, etc.) - để FE navigate
        public int? DataId { get; set; }

        // Staff tạo notification này (nếu có)
        public int? StaffId { get; set; }

        // User nhận notification
        public int UserId { get; set; }

        public DateTime Created { get; set; }

        // Constructors
        public Notification() { }

        public Notification(
            string title,
            string description,
            string dataType,
            int? dataId,
            int? staffId,
            int userId,
            DateTime created)
        {
            Title = title;
            Description = description;
            DataType = dataType;
            DataId = dataId;
            StaffId = staffId;
            UserId = userId;
            Created = created;
        }
    }
}