namespace BookingService.DTOs
{
    public class PeakHoursReportResponse
    {
        public int TotalOrders { get; set; }
        public List<HourlyOrderCount> HourlyData { get; set; } = new();
        public List<int> TopPeakHours { get; set; } = new();
        public string GeneratedAt { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class HourlyOrderCount
    {
        public int Hour { get; set; }
        public string TimeSlot { get; set; } = string.Empty; // "00:00 - 01:00"
        public int OrderCount { get; set; }
        public double Percentage { get; set; }
        public bool IsPeakHour { get; set; }
    }
}