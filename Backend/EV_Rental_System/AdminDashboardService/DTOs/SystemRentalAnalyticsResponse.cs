namespace AdminDashboardService.DTOs
{
    /// <summary>
    /// System-wide rental analytics (all users combined)
    /// </summary>
    public class SystemRentalAnalyticsResponse
    {
        // Overall Statistics
        public int TotalUsers { get; set; }
        public int TotalRentals { get; set; }
        public int CompletedRentals { get; set; }
        public int ActiveRentals { get; set; }

        // Financial
        public decimal TotalRevenue { get; set; }
        public decimal AverageRentalValue { get; set; }

        // Time-based Statistics
        public double AverageRentalDurationHours { get; set; }
        public int[] PeakRentalHours { get; set; } = Array.Empty<int>(); // Most common hours
        public Dictionary<string, int> RentalsByDayOfWeek { get; set; } = new();

        // Performance Metrics
        public double OnTimeReturnRate { get; set; } // Percentage
        public int TotalLateReturns { get; set; }
        public int TotalDamageReports { get; set; }

        // Trust Score
        public double AverageTrustScore { get; set; }

        // Popular Vehicles
        public List<PopularVehicleStats> MostRentedVehicles { get; set; } = new();
    }

    public class PopularVehicleStats
    {
        public int VehicleId { get; set; }
        public string VehicleName { get; set; } = string.Empty;
        public int RentalCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
