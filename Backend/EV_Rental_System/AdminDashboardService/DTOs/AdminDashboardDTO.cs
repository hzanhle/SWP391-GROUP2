namespace AdminDashboardService.DTOs
{
    public class AdminDashboardDTO
    {
        public decimal TotalRevenue { get; set; }
        public int TotalStations { get; set; }
        public int ActiveStations { get; set; }
        public int InactiveStations { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalEmployee { get; set; }
        public int TotalMembers { get; set; }
        public int TotalBookings { get; set; }

    }
}
