using AdminDashboardService.DTOs;
using AdminDashboardService.ExternalDbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdminDashboardService.Services
{
    public class RentalAnalyticsService : IRentalAnalyticsService
    {
        private readonly BookingServiceDbContext _bookingDb;
        private readonly UserServiceDbContext _userDb;
        private readonly ILogger<RentalAnalyticsService> _logger;

        public RentalAnalyticsService(
            BookingServiceDbContext bookingDb,
            UserServiceDbContext userDb,
            ILogger<RentalAnalyticsService> logger)
        {
            _bookingDb = bookingDb;
            _userDb = userDb;
            _logger = logger;
        }

        public async Task<UserRentalStatsResponse> GetUserRentalStatsAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Getting rental stats for User {UserId}", userId);

                // Get user info
                var user = await _userDb.Users.FindAsync(userId);
                if (user == null)
                {
                    throw new InvalidOperationException($"User {userId} not found");
                }

                // Get all orders for this user
                var orders = await _bookingDb.Orders
                    .Where(o => o.UserId == userId)
                    .ToListAsync();

                var completedOrders = orders.Where(o => o.Status == "Completed").ToList();
                var cancelledOrders = orders.Where(o => o.Status == "Cancelled").ToList();

                // Get settlements for damage and late return analysis
                var settlements = await _bookingDb.Settlements
                    .Where(s => orders.Select(o => o.OrderId).Contains(s.OrderId))
                    .ToListAsync();

                // Calculate statistics
                var totalSpent = completedOrders.Sum(o => o.TotalCost);
                var averageCost = completedOrders.Any() ? totalSpent / completedOrders.Count : 0;

                var durations = completedOrders
                    .Select(o => (o.ToDate - o.FromDate).TotalHours)
                    .ToList();
                var avgDuration = durations.Any() ? durations.Average() : 0;

                // Peak rental hours (most common start hours)
                var hourCounts = completedOrders
                    .GroupBy(o => o.FromDate.Hour)
                    .Select(g => new { Hour = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(3)
                    .Select(x => x.Hour)
                    .ToArray();

                // On-time return rate
                var lateReturns = settlements.Count(s => s.OvertimeHours > 0);
                var onTimeRate = completedOrders.Any()
                    ? ((completedOrders.Count - lateReturns) / (double)completedOrders.Count) * 100
                    : 100;

                // Damage analysis
                var rentalsWithDamage = settlements.Count(s => s.DamageCharge > 0);

                // Trust score
                var trustScore = await _bookingDb.TrustScores
                    .Where(t => t.UserId == userId)
                    .Select(t => t.Score)
                    .FirstOrDefaultAsync();

                // Most rented vehicle type
                var mostRentedVehicle = completedOrders
                    .GroupBy(o => o.VehicleId)
                    .Select(g => new { VehicleId = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .FirstOrDefault();

                return new UserRentalStatsResponse
                {
                    UserId = userId,
                    UserName = user.UserName ?? "",
                    Email = user.Email ?? "",
                    TotalRentals = orders.Count,
                    CompletedRentals = completedOrders.Count,
                    CancelledRentals = cancelledOrders.Count,
                    TotalSpent = totalSpent,
                    AverageRentalCost = averageCost,
                    AverageRentalDurationHours = avgDuration,
                    PeakRentalHours = hourCounts,
                    OnTimeReturnRate = onTimeRate,
                    RentalsWithDamage = rentalsWithDamage,
                    LateReturns = lateReturns,
                    CurrentTrustScore = trustScore,
                    MostRentedVehicleType = mostRentedVehicle != null ? $"Vehicle #{mostRentedVehicle.VehicleId}" : null,
                    MostRentedVehicleCount = mostRentedVehicle?.Count ?? 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rental stats for User {UserId}", userId);
                throw;
            }
        }

        public async Task<SystemRentalAnalyticsResponse> GetSystemRentalAnalyticsAsync()
        {
            try
            {
                _logger.LogInformation("Getting system-wide rental analytics");

                // Get all orders
                var allOrders = await _bookingDb.Orders.ToListAsync();
                var completedOrders = allOrders.Where(o => o.Status == "Completed").ToList();
                var activeOrders = allOrders.Where(o => o.Status == "InProgress").ToList();

                // Get all settlements
                var settlements = await _bookingDb.Settlements.ToListAsync();

                // Total users who have made rentals
                var totalUsers = allOrders.Select(o => o.UserId).Distinct().Count();

                // Financial statistics
                var totalRevenue = completedOrders.Sum(o => o.TotalCost);
                var averageValue = completedOrders.Any() ? totalRevenue / completedOrders.Count : 0;

                // Average rental duration
                var durations = completedOrders
                    .Select(o => (o.ToDate - o.FromDate).TotalHours)
                    .ToList();
                var avgDuration = durations.Any() ? durations.Average() : 0;

                // Peak rental hours (system-wide)
                var hourCounts = completedOrders
                    .GroupBy(o => o.FromDate.Hour)
                    .Select(g => new { Hour = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(5)
                    .Select(x => x.Hour)
                    .ToArray();

                // Rentals by day of week
                var rentalsByDay = completedOrders
                    .GroupBy(o => o.FromDate.DayOfWeek.ToString())
                    .ToDictionary(g => g.Key, g => g.Count());

                // Performance metrics
                var lateReturns = settlements.Count(s => s.OvertimeHours > 0);
                var onTimeRate = completedOrders.Any()
                    ? ((completedOrders.Count - lateReturns) / (double)completedOrders.Count) * 100
                    : 100;

                var damageReports = settlements.Count(s => s.DamageCharge > 0);

                // Average trust score
                var avgTrustScore = await _bookingDb.TrustScores
                    .AverageAsync(t => (double?)t.Score) ?? 0;

                // Most rented vehicles
                var mostRentedVehicles = completedOrders
                    .GroupBy(o => o.VehicleId)
                    .Select(g => new PopularVehicleStats
                    {
                        VehicleId = g.Key,
                        VehicleName = $"Vehicle #{g.Key}",
                        RentalCount = g.Count(),
                        TotalRevenue = g.Sum(o => o.TotalCost)
                    })
                    .OrderByDescending(v => v.RentalCount)
                    .Take(10)
                    .ToList();

                return new SystemRentalAnalyticsResponse
                {
                    TotalUsers = totalUsers,
                    TotalRentals = allOrders.Count,
                    CompletedRentals = completedOrders.Count,
                    ActiveRentals = activeOrders.Count,
                    TotalRevenue = totalRevenue,
                    AverageRentalValue = averageValue,
                    AverageRentalDurationHours = avgDuration,
                    PeakRentalHours = hourCounts,
                    RentalsByDayOfWeek = rentalsByDay,
                    OnTimeReturnRate = onTimeRate,
                    TotalLateReturns = lateReturns,
                    TotalDamageReports = damageReports,
                    AverageTrustScore = avgTrustScore,
                    MostRentedVehicles = mostRentedVehicles
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system rental analytics");
                throw;
            }
        }

        public async Task<PeakHoursResponse> GetPeakRentalHoursAsync()
        {
            try
            {
                _logger.LogInformation("Getting peak rental hours analysis");

                var completedOrders = await _bookingDb.Orders
                    .Where(o => o.Status == "Completed")
                    .ToListAsync();

                // Group by hour and count
                var hourlyStats = completedOrders
                    .GroupBy(o => o.FromDate.Hour)
                    .Select(g => new HourlyRentalStats
                    {
                        Hour = g.Key,
                        RentalCount = g.Count()
                    })
                    .OrderBy(h => h.Hour)
                    .ToList();

                // Fill missing hours with 0
                for (int hour = 0; hour < 24; hour++)
                {
                    if (!hourlyStats.Any(h => h.Hour == hour))
                    {
                        hourlyStats.Add(new HourlyRentalStats { Hour = hour, RentalCount = 0 });
                    }
                }
                hourlyStats = hourlyStats.OrderBy(h => h.Hour).ToList();

                // Top 3 peak hours
                var top3Peak = hourlyStats
                    .OrderByDescending(h => h.RentalCount)
                    .Take(3)
                    .Select(h => h.Hour)
                    .ToArray();

                // Top 3 low hours
                var top3Low = hourlyStats
                    .OrderBy(h => h.RentalCount)
                    .Take(3)
                    .Select(h => h.Hour)
                    .ToArray();

                return new PeakHoursResponse
                {
                    HourlyStats = hourlyStats,
                    Top3PeakHours = top3Peak,
                    Top3LowHours = top3Low
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting peak rental hours");
                throw;
            }
        }
    }
}
