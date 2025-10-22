using AdminDashboardService.DTOs;
using AdminDashboardService.ExternalDbContexts;
using Microsoft.EntityFrameworkCore;
using AdminDashboardService.Data;

namespace AdminDashboardService.Repositories
{
    public class AdminDashboardRepository : IAdminDashboardRepository
    {
        private readonly MyDbContext _context;
        private readonly UserServiceDbContext _userDb;
        private readonly StationServiceDbContext _stationDb;
        private readonly TwoWheelVehicleServiceDbContext _vehicleDb;
        private readonly BookingServiceDbContext _bookingDb;
        private readonly ILogger<AdminDashboardRepository> _logger;

        public AdminDashboardRepository(
            MyDbContext context,
            UserServiceDbContext userDb,
            StationServiceDbContext stationDb,
            TwoWheelVehicleServiceDbContext vehicleDb,
            BookingServiceDbContext bookingDb,
            ILogger<AdminDashboardRepository> logger)
        {
            _context = context;
            _userDb = userDb;
            _stationDb = stationDb;
            _vehicleDb = vehicleDb;
            _bookingDb = bookingDb;
            _logger = logger;
        }

        public async Task<DashboardSummaryDTO> GetDashboardSummaryAsync()
        {
            _logger.LogInformation("Đang lấy dữ liệu tóm tắt cho bảng Dashboard từ cơ sở dữ liệu");

            try
            {
                // Truy vấn tuần tự
                var totalUsers = await GetTotalUsersAsync();
                var usersByRole = await GetTotalUsersByRoleAsync();
                var totalStations = await GetTotalStationsAsync();
                var totalVehicles = await GetTotalVehiclesAsync();
                var vehiclesByStatus = await GetVehiclesByStatusAsync();
                var totalBookings = await GetTotalBookingsAsync();
                var bookingsByStatus = await GetBookingsByStatusAsync();
                var totalRevenue = await GetTotalRevenueAsync();
                var pendingApprovals = await GetPendingApprovalsAsync();

                var summary = new DashboardSummaryDTO
                {
                    TotalUsers = totalUsers,
                    TotalUsersByRole = usersByRole,
                    TotalStations = totalStations,
                    TotalVehicles = totalVehicles,
                    VehiclesByStatus = vehiclesByStatus,
                    TotalBookings = totalBookings,
                    BookingsByStatus = bookingsByStatus,
                    TotalRevenue = totalRevenue,
                    PendingCitizenApprovals = pendingApprovals.PendingCitizen,
                    PendingLicenseApprovals = pendingApprovals.PendingLicense
                };

                _logger.LogInformation("Dashboard summary fetched successfully");
                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard summary");
                throw;
            }
        }

        // ==================== Private Helper Methods ====================

        private async Task<int> GetTotalUsersAsync()
        {
            return await _userDb.Users.CountAsync();
        }

        private async Task<Dictionary<string, int>> GetTotalUsersByRoleAsync()
        {
            var usersByRole = await _userDb.Users
                .Include(u => u.Role)
                .Where(u => u.Role != null)
                .GroupBy(u => u.Role!.RoleName)
                .Select(g => new { RoleName = g.Key, Count = g.Count() })
                .ToListAsync();

            return usersByRole.ToDictionary(x => x.RoleName, x => x.Count);
        }

        private async Task<int> GetTotalStationsAsync()
        {
            return await _stationDb.Stations
                .Where(s => s.IsActive)
                .CountAsync();
        }

        private async Task<int> GetTotalVehiclesAsync()
        {
            return await _vehicleDb.Vehicles.CountAsync();
        }

        private async Task<Dictionary<string, int>> GetVehiclesByStatusAsync()
        {
            var vehiclesByStatus = await _vehicleDb.Vehicles
                .GroupBy(v => v.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return vehiclesByStatus.ToDictionary(x => x.Status, x => x.Count);
        }

        private async Task<int> GetTotalBookingsAsync()
        {
            return await _bookingDb.Orders.CountAsync();
        }

        private async Task<Dictionary<string, int>> GetBookingsByStatusAsync()
        {
            var bookingsByStatus = await _bookingDb.Orders
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return bookingsByStatus.ToDictionary(x => x.Status, x => x.Count);
        }

        private async Task<decimal> GetTotalRevenueAsync()
        {
            var totalRevenue = await _bookingDb.Payments
                .Where(p => p.IsFullyPaid)
                .SumAsync(p => (decimal?)p.PaidAmount) ?? 0;

            return totalRevenue;
        }

        private async Task<(int PendingCitizen, int PendingLicense)> GetPendingApprovalsAsync()
        {
            var pendingCitizen = await _userDb.CitizenInfos
                .Where(c => c.IsApproved == false || c.IsApproved == null)
                .CountAsync();

            var pendingLicense = await _userDb.DriverLicenses
                .Where(d => !d.IsApproved)
                .CountAsync();

            return (pendingCitizen, pendingLicense);
        }

        // ==================== Additional Statistics Methods ====================

        public async Task<List<StationStatisticDTO>> GetStationStatisticsAsync()
        {
            _logger.LogInformation("Fetching station statistics");

            var stations = await _stationDb.Stations
                .Where(s => s.IsActive)
                .ToListAsync();

            var stationStats = new List<StationStatisticDTO>();

            foreach (var station in stations)
            {
                var vehicleCount = await _vehicleDb.Vehicles
                    .Where(v => v.StationId == station.Id && v.IsActive)
                    .CountAsync();

                var staffCount = await _stationDb.StaffShifts
                    .Where(ss => ss.StationId == station.Id)
                    .Select(ss => ss.UserId)
                    .Distinct()
                    .CountAsync();

                stationStats.Add(new StationStatisticDTO
                {
                    StationId = station.Id,
                    StationName = station.Name,
                    Location = station.Location,
                    TotalVehicles = vehicleCount,
                    TotalStaff = staffCount
                });
            }

            return stationStats;
        }

        public async Task<List<RevenueByMonthDTO>> GetRevenueByMonthAsync(int year)
        {
            _logger.LogInformation("Fetching revenue by month for year {Year}", year);

            var revenueByMonth = await _bookingDb.Payments
                .Where(p => p.IsFullyPaid && p.FullPaymentDate.HasValue
                    && p.FullPaymentDate.Value.Year == year)
                .GroupBy(p => p.FullPaymentDate!.Value.Month)
                .Select(g => new RevenueByMonthDTO
                {
                    Month = g.Key,
                    TotalRevenue = g.Sum(p => p.PaidAmount)
                })
                .OrderBy(r => r.Month)
                .ToListAsync();

            return revenueByMonth;
        }

        public async Task<List<VehicleUsageDTO>> GetTopUsedVehiclesAsync(int top = 10)
        {
            _logger.LogInformation("Fetching top {Count} used vehicles", top);

            var topVehicles = await _bookingDb.Orders
                .GroupBy(o => o.VehicleId)
                .Select(g => new VehicleUsageDTO
                {
                    VehicleId = g.Key,
                    TotalBookings = g.Count(),
                    TotalRevenue = g.Sum(o => o.TotalCost)
                })
                .OrderByDescending(v => v.TotalBookings)
                .Take(top)
                .ToListAsync();

            return topVehicles;
        }

        public async Task<UserGrowthStatisticsDTO> GetUserGrowthStatisticsAsync()
        {
            _logger.LogInformation("Fetching user growth statistics");

            var now = DateTime.UtcNow;
            var lastMonth = now.AddMonths(-1);
            var lastYear = now.AddYears(-1);

            var totalUsers = await _userDb.Users.CountAsync();
            var newUsersThisMonth = await _userDb.Users
                .Where(u => u.CreatedAt >= new DateTime(now.Year, now.Month, 1))
                .CountAsync();
            var newUsersLastMonth = await _userDb.Users
                .Where(u => u.CreatedAt >= new DateTime(lastMonth.Year, lastMonth.Month, 1)
                    && u.CreatedAt < new DateTime(now.Year, now.Month, 1))
                .CountAsync();

            return new UserGrowthStatisticsDTO
            {
                TotalUsers = totalUsers,
                NewUsersThisMonth = newUsersThisMonth,
                NewUsersLastMonth = newUsersLastMonth,
                GrowthRate = newUsersLastMonth > 0
                    ? ((double)(newUsersThisMonth - newUsersLastMonth) / newUsersLastMonth) * 100
                    : 0
            };
        }
    }
}