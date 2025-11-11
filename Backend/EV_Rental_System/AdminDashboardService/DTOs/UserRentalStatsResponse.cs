namespace AdminDashboardService.DTOs
{
    /// <summary>
    /// Individual user's rental statistics
    /// For admin to view detailed analytics of a specific user
    /// </summary>
    public class UserRentalStatsResponse
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Rental Statistics
        public int TotalRentals { get; set; }
        public int CompletedRentals { get; set; }
        public int CancelledRentals { get; set; }

        // Financial Statistics
        public decimal TotalSpent { get; set; }
        public decimal AverageRentalCost { get; set; }

        // Rental Behavior
        public double AverageRentalDurationHours { get; set; }
        public int[] PeakRentalHours { get; set; } = Array.Empty<int>(); // Hours of day (0-23)
        public double OnTimeReturnRate { get; set; } // Percentage (0-100)

        // Damage & Issues
        public int RentalsWithDamage { get; set; }
        public int LateReturns { get; set; }

        // Trust Score
        public int CurrentTrustScore { get; set; }

        // Most Rented Vehicle
        public string? MostRentedVehicleType { get; set; }
        public int MostRentedVehicleCount { get; set; }
    }
}
