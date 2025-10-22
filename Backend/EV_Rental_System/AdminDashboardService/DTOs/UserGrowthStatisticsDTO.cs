namespace AdminDashboardService.DTOs
{
    public class UserGrowthStatisticsDTO
    {
        public int TotalUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int NewUsersLastMonth { get; set; }
        public double GrowthRate { get; set; } // Tỷ lệ phần trăm
    }
}