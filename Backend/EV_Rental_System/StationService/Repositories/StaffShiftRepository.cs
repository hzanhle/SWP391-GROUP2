using Microsoft.EntityFrameworkCore;
using StationService.DTOs.StaffShift;
using StationService.Models;

namespace StationService.Repositories
{
    public class StaffShiftRepository : IStaffShiftRepository
    {
        private readonly MyDbContext _context;
        private readonly ILogger<StaffShiftRepository> _logger;

        public StaffShiftRepository(MyDbContext context, ILogger<StaffShiftRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ==================== CRUD ====================

        public async Task<StaffShift?> GetByIdAsync(int id)
        {
            return await _context.StaffShifts
                .Include(s => s.Station)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<StaffShift>> GetAllAsync()
        {
            return await _context.StaffShifts
                .Include(s => s.Station)
                .OrderByDescending(s => s.ShiftDate)
                .ThenBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<StaffShift> CreateAsync(StaffShift shift)
        {
            _context.StaffShifts.Add(shift);
            await _context.SaveChangesAsync();
            return await _context.StaffShifts
                        .Include(s => s.Station)
                        .FirstAsync(s => s.Id == shift.Id);
        }

        public async Task<StaffShift> UpdateAsync(StaffShift shift)
        {
            shift.UpdatedAt = DateTime.UtcNow;
            _context.StaffShifts.Update(shift);
            await _context.SaveChangesAsync();
            return shift;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var shift = await GetByIdAsync(id);
            if (shift == null) return false;

            _context.StaffShifts.Remove(shift);
            await _context.SaveChangesAsync();
            return true;
        }

        // ==================== PHƯƠNG THỨC TRUY XUẤT DỰ LIỆU ====================

        public async Task<List<StaffShift>> GetShiftsByUserIdAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.StaffShifts
                .Include(s => s.Station)
                .Where(s => s.UserId == userId);

            if (fromDate.HasValue)
                query = query.Where(s => s.ShiftDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(s => s.ShiftDate <= toDate.Value);

            return await query
                .OrderBy(s => s.ShiftDate)
                .ThenBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<List<StaffShift>> GetShiftsByStationIdAsync(int stationId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.StaffShifts
                .Include(s => s.Station)
                .Where(s => s.StationId == stationId);

            if (fromDate.HasValue)
                query = query.Where(s => s.ShiftDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(s => s.ShiftDate <= toDate.Value);

            return await query
                .OrderBy(s => s.ShiftDate)
                .ThenBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<List<StaffShift>> GetShiftsByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.StaffShifts
                .Include(s => s.Station)
                .Where(s => s.ShiftDate >= fromDate && s.ShiftDate <= toDate)
                .OrderBy(s => s.ShiftDate)
                .ThenBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<List<StaffShift>> GetShiftsByStatusAsync(string status)
        {
            return await _context.StaffShifts
                .Include(s => s.Station)
                .Where(s => s.Status == status)
                .OrderBy(s => s.ShiftDate)
                .ToListAsync();
        }

        // ==================== LOGIC NGHIỆP VỤ ====================

        public async Task<bool> HasConflictingShiftAsync(int userId, DateTime shiftDate, TimeSpan startTime, TimeSpan endTime, int? excludeShiftId = null)
        {
            var query = _context.StaffShifts
                .Where(s => s.UserId == userId
                    && s.ShiftDate.Date == shiftDate.Date
                    && s.Status != "Cancelled"
                    && s.Status != "NoShow");

            if (excludeShiftId.HasValue)
                query = query.Where(s => s.Id != excludeShiftId.Value);

            var existingShifts = await query.ToListAsync();

            foreach (var shift in existingShifts)
            {
                if (startTime < shift.EndTime && endTime > shift.StartTime)
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<int> GetShiftCountForUserOnDateAsync(int userId, DateTime date)
        {
            return await _context.StaffShifts
                .Where(s => s.UserId == userId
                    && s.ShiftDate.Date == date.Date
                    && s.Status != "Cancelled"
                    && s.Status != "NoShow")
                .CountAsync();
        }

        public async Task<bool> IsUserAvailableForShiftAsync(int userId, DateTime shiftDate, TimeSpan startTime, TimeSpan endTime)
        {
            var shiftCount = await GetShiftCountForUserOnDateAsync(userId, shiftDate);
            if (shiftCount >= 2)
                return false;

            var hasConflict = await HasConflictingShiftAsync(userId, shiftDate, startTime, endTime);
            if (hasConflict)
                return false;

            return true;
        }

        // ==================== Check-in/out ====================

        private static DateTime NormalizeToUtc(DateTime localTime)
        {
            if (localTime.Kind == DateTimeKind.Utc)
            {
                return localTime;
            }

            // Assume localTime is in SE Asia Standard Time (UTC+7)
            var utc = DateTime.SpecifyKind(localTime, DateTimeKind.Local).ToUniversalTime();
            return utc;
        }

        public async Task<bool> CheckInAsync(int shiftId, DateTime checkInTime)
        {
            var shift = await GetByIdAsync(shiftId);
            if (shift == null || !string.Equals(shift.Status?.Trim(), "Scheduled", StringComparison.OrdinalIgnoreCase))
                return false;

            shift.ActualCheckInTime = NormalizeToUtc(checkInTime);
            shift.Status = "CheckedIn";
            shift.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CheckOutAsync(int shiftId, DateTime checkOutTime)
        {
            var shift = await GetByIdAsync(shiftId);
            if (shift == null || !string.Equals(shift.Status?.Trim(), "CheckedIn", StringComparison.OrdinalIgnoreCase))
                return false;

            shift.ActualCheckOutTime = NormalizeToUtc(checkOutTime);
            shift.Status = "Completed";
            shift.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ==================== ĐỔI CA ====================

        public async Task<bool> SwapShiftUsersAsync(int shift1Id, int shift2Id)
        {
            var shift1 = await GetByIdAsync(shift1Id);
            var shift2 = await GetByIdAsync(shift2Id);

            if (shift1 == null || shift2 == null)
                return false;

            var tempUserId = shift1.UserId;
            shift1.UserId = shift2.UserId;
            shift2.UserId = tempUserId;

            shift1.UpdatedAt = DateTime.UtcNow;
            shift2.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ==================== THỐNG KÊ ====================

        public async Task<MonthlyTimesheetDTO> GetMonthlyTimesheetAsync(int userId, int month, int year)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var shifts = await GetShiftsByUserIdAsync(userId, startDate, endDate);

            var timesheet = new MonthlyTimesheetDTO
            {
                UserId = userId,
                Month = month,
                Year = year,
                TotalShifts = shifts.Count,
                CompletedShifts = shifts.Count(s => s.Status == "Completed"),
                TotalHoursScheduled = shifts.Sum(s => s.ScheduledDuration.TotalHours),
                TotalHoursWorked = shifts.Where(s => s.ActualDuration.HasValue)
                                        .Sum(s => s.ActualDuration!.Value.TotalHours),
                Shifts = shifts.Select(s => new ShiftSummaryDTO
                {
                    ShiftId = s.Id,
                    Date = s.ShiftDate,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Status = s.Status,
                    HoursWorked = s.ActualDuration?.TotalHours ?? 0
                }).ToList()
            };

            return timesheet;
        }

        public async Task<StationShiftStatisticsDTO> GetStationStatisticsAsync(int stationId, DateTime fromDate, DateTime toDate)
        {
            var shifts = await GetShiftsByStationIdAsync(stationId, fromDate, toDate);

            var statistics = new StationShiftStatisticsDTO
            {
                StationId = stationId,
                FromDate = fromDate,
                ToDate = toDate,
                TotalShifts = shifts.Count,
                CompletedShifts = shifts.Count(s => s.Status == "Completed"),
                CancelledShifts = shifts.Count(s => s.Status == "Cancelled"),
                NoShowShifts = shifts.Count(s => s.Status == "NoShow"),
                EmployeeSummaries = shifts
                    .GroupBy(s => s.UserId)
                    .Select(g => new EmployeeShiftSummary
                    {
                        UserId = g.Key,
                        TotalShifts = g.Count(),
                        CompletedShifts = g.Count(s => s.Status == "Completed"),
                        TotalHours = g.Where(s => s.ActualDuration.HasValue)
                                     .Sum(s => s.ActualDuration!.Value.TotalHours)
                    }).ToList()
            };

            return statistics;
        }
    }

}
