namespace AdminDashboardService.DTOs
{
    /// <summary>
    /// Peak rental hours analysis
    /// </summary>
    public class PeakHoursResponse
    {
        public List<HourlyRentalStats> HourlyStats { get; set; } = new();
        public int[] Top3PeakHours { get; set; } = Array.Empty<int>();
        public int[] Top3LowHours { get; set; } = Array.Empty<int>();
    }

    public class HourlyRentalStats
    {
        public int Hour { get; set; } // 0-23
        public int RentalCount { get; set; }
        public string Label => $"{Hour}:00 - {(Hour + 1) % 24}:00";
    }
}
