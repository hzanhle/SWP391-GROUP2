using StationService.DTOs.StaffShift;
using StationService.Models;

namespace StationService.Repositories
{
    public interface IStaffShiftRepository
    {
        // Phương thức CRUD
        Task<StaffShift?> GetByIdAsync(int id);
        Task<List<StaffShift>> GetAllAsync();
        Task<StaffShift> CreateAsync(StaffShift shift);
        Task<StaffShift> UpdateAsync(StaffShift shift);
        Task<bool> DeleteAsync(int id);

        // Phương thức truy vấn
        Task<List<StaffShift>> GetShiftsByUserIdAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<StaffShift>> GetShiftsByStationIdAsync(int stationId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<StaffShift>> GetShiftsByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<List<StaffShift>> GetShiftsByStatusAsync(string status);

        // Logic nghiệp vụ
        Task<bool> HasConflictingShiftAsync(int userId, DateTime shiftDate, TimeSpan startTime, TimeSpan endTime, int? excludeShiftId = null);
        Task<int> GetShiftCountForUserOnDateAsync(int userId, DateTime date);
        Task<bool> IsUserAvailableForShiftAsync(int userId, DateTime shiftDate, TimeSpan startTime, TimeSpan endTime);

        // Check-in/out
        Task<bool> CheckInAsync(int shiftId, DateTime checkInTime);
        Task<bool> CheckOutAsync(int shiftId, DateTime checkOutTime);

        // Trao đổi ca làm việc
        Task<bool> SwapShiftUsersAsync(int shift1Id, int shift2Id);

        // Thống kê
        Task<MonthlyTimesheetDTO> GetMonthlyTimesheetAsync(int userId, int month, int year);
        Task<StationShiftStatisticsDTO> GetStationStatisticsAsync(int stationId, DateTime fromDate, DateTime toDate);
    }
}
