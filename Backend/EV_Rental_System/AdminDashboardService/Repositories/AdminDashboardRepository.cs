// File: AdminDashboardService/Repositories/AdminDashboardRepository.cs

using AdminDashboardService.Data;
using AdminDashboardService.DTOs; // Sử dụng DTO mới
using BookingSerivce.Models;
using Microsoft.EntityFrameworkCore;
using StationService.Models;       // Thêm using cho Station model
using TwoWheelVehicleService.Models;
using UserService.Models;
using System.Linq;
using System.Threading.Tasks;

namespace AdminDashboardService.Repositories
{
    public class AdminDashboardRepository : IAdminDashboardRepository
    {
        private readonly MyDbContext _context;

        public AdminDashboardRepository(MyDbContext context)
        {
            _context = context;
        }

        // Đảm bảo kiểu trả về là AdminDashboardDTO
        public async Task<AdminDashboardDTO> GetDashboardMetricsAsync()
        {
            // === TÍNH TOÁN CÁC CHỈ SỐ MỚI ===

            // 1. Doanh thu từ BookingService
            var totalRevenue = await _context.OnlineContracts
                                             .Where(c => c.Status == "Signed" && c.Order != null)
                                             .Select(c => c.Order!.TotalCost)
                                             .SumAsync();

            // 2. Thống kê Stations từ StationService
            var totalStations = await _context.Stations.CountAsync();
            var activeStations = await _context.Stations.CountAsync(s => s.IsActive == true);
            var inactiveStations = totalStations - activeStations;

            // 3. Thống kê Users theo Role từ UserService (Bác hãy điều chỉnh lại tên Role cho đúng)
            var totalAdmins = await _context.Users.CountAsync(u => u.RoleId == 3);
            var totalEmployee = await _context.Users.CountAsync(u => u.RoleId == 2);
            var totalMembers = await _context.Users.CountAsync(u => u.RoleId == 1);

            // 4. Tổng số lượt thuê thành công (hợp đồng đã ký)
            var totalBookings = await _context.OnlineContracts.CountAsync(c => c.Status == "Signed");

            // === TẠO DTO TRẢ VỀ ===
            return new AdminDashboardDTO
            {
                TotalRevenue = totalRevenue,
                TotalStations = totalStations,
                ActiveStations = activeStations,
                InactiveStations = inactiveStations,
                TotalAdmins = totalAdmins,
                TotalEmployee = totalEmployee,
                TotalMembers = totalMembers,
                TotalBookings = totalBookings
            };
        }
    }
}