using AdminDashboardService.DTOs;
using AdminDashboardService.Repositories;
using Microsoft.Extensions.Logging;
using System;                        
using System.Threading.Tasks;

namespace AdminDashboardService.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IAdminDashboardRepository _repository;
        private readonly ILogger<AdminDashboardService> _logger;

        public AdminDashboardService(
            IAdminDashboardRepository repository,
            ILogger<AdminDashboardService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<DashboardSummaryDTO> GetDashboardSummaryAsync()
        {
            _logger.LogInformation("Dịch vụ: Ghi nhận tóm tắt bảng điều khiển.");
            try
            {
                var summary = await _repository.GetDashboardSummaryAsync();
                _logger.LogInformation("Dịch vụ: Bảng điều khiển đã được truy xuất thành công.");
                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dịch vụ: Đã xảy ra lỗi khi ghi nhận tóm tắt bảng điều khiển");
                throw;
            }
        }

        public async Task<List<StationStatisticDTO>> GetStationStatisticsAsync()
        {
            _logger.LogInformation("Dịch vụ: Nhận số liệu thống kê trạm");
            try
            {
                var stats = await _repository.GetStationStatisticsAsync();
                _logger.LogInformation("Dịch vụ: Đã truy xuất thành công số liệu thống kê của trạm");
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dịch vụ: Lỗi khi lấy số liệu thống kê trạm");
                throw;
            }
        }

        public async Task<List<RevenueByMonthDTO>> GetRevenueByMonthAsync(int year)
        {
            _logger.LogInformation("Dịch vụ: Nhận doanh thu theo tháng trong năm {Year}", year);
            try
            {
                var revenue = await _repository.GetRevenueByMonthAsync(year);
                _logger.LogInformation("Dịch vụ: Doanh thu theo tháng đã được truy xuất thành công");
                return revenue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dịch vụ: Lỗi khi lấy doanh thu theo tháng");
                throw;
            }
        }

        public async Task<List<VehicleUsageDTO>> GetTopUsedVehiclesAsync(int top = 10)
        {
            _logger.LogInformation("Dịch vụ: Tìm kiếm {Count} xe được thuê nhiều nhất", top);
            try
            {
                var vehicles = await _repository.GetTopUsedVehiclesAsync(top);
                _logger.LogInformation("Dịch vụ: Đã tìm lại thành công những chiếc xe được thuê nhiều nhất");
                return vehicles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dịch vụ: Lỗi khi tìm kiếm xe đã được thuê");
                throw;
            }
        }

        public async Task<UserGrowthStatisticsDTO> GetUserGrowthStatisticsAsync()
        {
            _logger.LogInformation("Dịch vụ: Nhận số liệu thống kê về sự tăng trưởng của người dùng");
            try
            {
                var stats = await _repository.GetUserGrowthStatisticsAsync();
                _logger.LogInformation("Dịch vụ: Đã truy xuất thành công số liệu thống kê tăng trưởng người dùng");
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dịch vụ: Lỗi khi lấy số liệu thống kê về mức tăng trưởng người dùng");
                throw;
            }
        }
    }
}