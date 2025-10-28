namespace BookingService.Models
{
    public class Feedback
    {
        public int FeedbackId { get; set; }
        public int UserId { get; set; }
        public int OrderId { get; set; }
        public Order? Order { get; set; }  // Navigation property đến Order
        public int VehicleId { get; set; }
        public double Rating { get; set; }
        public string? Comments { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;

        // 🔹 Constructor mặc định (EF cần)
        public Feedback() { }

        // 🔹 Constructor tiện dụng khi khởi tạo mới Feedback
        public Feedback(int userId, int orderId, int vehicleId, double vehicleRating, string? comments = null)
        {
            UserId = userId;
            OrderId = orderId;
            VehicleId = vehicleId;
            Rating = vehicleRating;
            Comments = comments;
        }
    }
}
