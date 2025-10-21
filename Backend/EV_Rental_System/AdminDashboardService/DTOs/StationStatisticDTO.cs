namespace AdminDashboardService.DTOs
{
    public class StationStatisticDTO
    {
        public int StationId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int TotalVehicles { get; set; }
        public int TotalStaff { get; set; }
    }
}