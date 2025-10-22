namespace AdminDashboardService.DTOs
{
    public class DashboardSummaryDTO
    {
        // Thống kê người dùng (Bao gồm theo role)
        public int TotalUsers { get; set; }
        public Dictionary<string, int> TotalUsersByRole { get; set; } = new();
        public int PendingCitizenApprovals { get; set; } // Thông tin đang chờ duyệt
        public int PendingLicenseApprovals { get; set; } // Thông tin đang chờ duyệt

        // Thống kê trạm
        public int TotalStations { get; set; }

        // Thống kê xe
        public int TotalVehicles { get; set; }
        public Dictionary<string, int> VehiclesByStatus { get; set; } = new();

        // hống kê lượt đặt xe
        public int TotalBookings { get; set; }
        public Dictionary<string, int> BookingsByStatus { get; set; } = new();

        // Thống kê tổng chi phí thuê
        public decimal TotalRevenue { get; set; }
    }
}