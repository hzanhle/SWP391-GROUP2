using StationService.DTOs.StaffShift;
using StationService.Models;
using StationService.Repositories;

namespace StationService.Services
{
    public class StaffShiftService : IStaffShiftService
    {
        private readonly IStaffShiftRepository _repository;
        private readonly IUserIntegrationService _userIntegrationService;
        private readonly ILogger<StaffShiftService> _logger;

        public StaffShiftService(
            IStaffShiftRepository repository,
            IUserIntegrationService userIntegrationService,
            ILogger<StaffShiftService> logger)
        {
            _repository = repository;
            _userIntegrationService = userIntegrationService;
            _logger = logger;
        }

        private static DateTime GetVietnamNow()
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            }
            catch (TimeZoneNotFoundException)
            {
                return DateTime.UtcNow.AddHours(7);
            }
            catch (InvalidTimeZoneException)
            {
                return DateTime.UtcNow.AddHours(7);
            }
        }

        public async Task<StaffShiftResponseDTO?> GetShiftByIdAsync(int id)
        {
            _logger.LogInformation("Getting shift with ID: {Id}", id);
            var shift = await _repository.GetByIdAsync(id);
            return shift == null ? null : MapToResponseDTO(shift);
        }

        public async Task<List<StaffShiftResponseDTO>> GetAllShiftsAsync()
        {
            _logger.LogInformation("Getting all shifts");
            var shifts = await _repository.GetAllAsync();
            return shifts.Select(MapToResponseDTO).ToList();
        }

        public async Task<StaffShiftResponseDTO> CreateShiftAsync(CreateStaffShiftDTO dto)
        {
            _logger.LogInformation("Creating new shift for User {UserId} at Station {StationId}",
                dto.UserId, dto.StationId);

            var duration = dto.EndTime - dto.StartTime;
            if (duration.TotalHours < 6 || duration.TotalHours > 8)
            {
                throw new ArgumentException("Ca làm việc phải từ 6-8 giờ");
            }

            var isAvailable = await _repository.IsUserAvailableForShiftAsync(
                dto.UserId, dto.ShiftDate, dto.StartTime, dto.EndTime);

            if (!isAvailable)
            {
                throw new InvalidOperationException(
                    "Nhân viên không thể nhận ca này (đã đủ 2 ca/ngày hoặc trùng lịch)");
            }

            var shift = new StaffShift
            {
                UserId = dto.UserId,
                StationId = dto.StationId,
                ShiftDate = dto.ShiftDate,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Notes = dto.Notes,
                Status = "Scheduled"
            };

            var created = await _repository.CreateAsync(shift);
            _logger.LogInformation("Shift created successfully with ID: {Id}", created.Id);

            return MapToResponseDTO(created);
        }

        public async Task<StaffShiftResponseDTO> UpdateShiftAsync(int id, UpdateStaffShiftDTO dto)
        {
            _logger.LogInformation("Updating shift {Id}", id);

            var shift = await _repository.GetByIdAsync(id);
            if (shift == null)
            {
                throw new KeyNotFoundException($"Shift with ID {id} not found");
            }

            if (dto.UserId.HasValue && dto.UserId.Value != shift.UserId)
            {
                var isAvailable = await _repository.IsUserAvailableForShiftAsync(
                    dto.UserId.Value,
                    dto.ShiftDate ?? shift.ShiftDate,
                    dto.StartTime ?? shift.StartTime,
                    dto.EndTime ?? shift.EndTime);

                if (!isAvailable)
                {
                    throw new InvalidOperationException("Nhân viên mới không thể nhận ca này");
                }

                shift.UserId = dto.UserId.Value;
            }

            if (dto.ShiftDate.HasValue) shift.ShiftDate = dto.ShiftDate.Value;
            if (dto.StartTime.HasValue) shift.StartTime = dto.StartTime.Value;
            if (dto.EndTime.HasValue) shift.EndTime = dto.EndTime.Value;
            if (dto.Status != null) shift.Status = dto.Status;
            if (dto.Notes != null) shift.Notes = dto.Notes;

            if (!shift.IsValidShiftDuration())
            {
                throw new ArgumentException("Ca làm việc phải từ 6-8 giờ");
            }

            var updated = await _repository.UpdateAsync(shift);
            _logger.LogInformation("Shift {Id} updated successfully", id);

            return MapToResponseDTO(updated);
        }

        public async Task<bool> DeleteShiftAsync(int id)
        {
            _logger.LogInformation("Deleting shift {Id}", id);

            var shift = await _repository.GetByIdAsync(id);
            if (shift == null)
            {
                throw new KeyNotFoundException($"Shift with ID {id} not found");
            }

            if (shift.Status == "CheckedIn" || shift.Status == "Completed")
            {
                throw new InvalidOperationException("Không thể xóa ca đang hoặc đã hoàn thành");
            }

            var result = await _repository.DeleteAsync(id);
            _logger.LogInformation("Shift {Id} deleted successfully", id);

            return result;
        }

        public async Task<List<StaffShiftResponseDTO>> GetShiftsByUserIdAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            _logger.LogInformation("Getting shifts for User {UserId}", userId);
            var shifts = await _repository.GetShiftsByUserIdAsync(userId, fromDate, toDate);
            return shifts.Select(MapToResponseDTO).ToList();
        }

        public async Task<List<StaffShiftResponseDTO>> GetShiftsByStationIdAsync(int stationId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            _logger.LogInformation("Getting shifts for Station {StationId}", stationId);
            var shifts = await _repository.GetShiftsByStationIdAsync(stationId, fromDate, toDate);
            return shifts.Select(MapToResponseDTO).ToList();
        }

        public async Task<bool> CheckInAsync(CheckInOutDTO dto)
        {
            _logger.LogInformation("User {UserId} checking in for shift {ShiftId}", dto.UserId, dto.ShiftId);

            var shift = await _repository.GetByIdAsync(dto.ShiftId);
            if (shift == null)
            {
                throw new KeyNotFoundException("Shift not found");
            }

            if (shift.UserId != dto.UserId)
            {
                throw new UnauthorizedAccessException("Bạn không phải nhân viên của ca này");
            }

            if (!string.Equals(shift.Status?.Trim(), "Scheduled", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Ca làm việc không ở trạng thái Scheduled");
            }

            var checkInTime = GetVietnamNow();

            var result = await _repository.CheckInAsync(dto.ShiftId, checkInTime);
            _logger.LogInformation("Check-in successful for shift {ShiftId}", dto.ShiftId);

            return result;
        }

        public async Task<bool> CheckOutAsync(CheckInOutDTO dto)
        {
            _logger.LogInformation("User {UserId} checking out for shift {ShiftId}", dto.UserId, dto.ShiftId);

            var shift = await _repository.GetByIdAsync(dto.ShiftId);
            if (shift == null)
            {
                throw new KeyNotFoundException("Shift not found");
            }

            if (shift.UserId != dto.UserId)
            {
                throw new UnauthorizedAccessException("Bạn không phải nhân viên của ca này");
            }

            if (!string.Equals(shift.Status?.Trim(), "CheckedIn", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Ca làm việc chưa check-in");
            }

            var checkOutTime = GetVietnamNow();

            var result = await _repository.CheckOutAsync(dto.ShiftId, checkOutTime);
            _logger.LogInformation("Check-out successful for shift {ShiftId}", dto.ShiftId);

            return result;
        }

        public async Task<bool> SwapShiftsAsync(int shift1Id, int shift2Id, int requestingUserId)
        {
            _logger.LogInformation("Swapping shifts {Shift1Id} and {Shift2Id}", shift1Id, shift2Id);

            var shift1 = await _repository.GetByIdAsync(shift1Id);
            var shift2 = await _repository.GetByIdAsync(shift2Id);

            if (shift1 == null || shift2 == null)
            {
                throw new KeyNotFoundException("Một trong hai ca không tồn tại");
            }

            if (shift1.Status != "Scheduled" || shift2.Status != "Scheduled")
            {
                throw new InvalidOperationException("Chỉ có thể đổi ca ở trạng thái Scheduled");
            }

            var user1Id = shift1.UserId;
            var user2Id = shift2.UserId;

            var hasConflict1 = await _repository.HasConflictingShiftAsync(
                user2Id, shift1.ShiftDate, shift1.StartTime, shift1.EndTime, shift1.Id);

            var hasConflict2 = await _repository.HasConflictingShiftAsync(
                user1Id, shift2.ShiftDate, shift2.StartTime, shift2.EndTime, shift2.Id);

            if (hasConflict1 || hasConflict2)
            {
                throw new InvalidOperationException("Không thể đổi ca do xung đột lịch");
            }

            var result = await _repository.SwapShiftUsersAsync(shift1Id, shift2Id);
            _logger.LogInformation("Shifts swapped successfully");

            return result;
        }

        public async Task<MonthlyTimesheetDTO> GetMonthlyTimesheetAsync(int userId, int month, int year)
        {
            _logger.LogInformation("Getting timesheet for User {UserId}, Month {Month}, Year {Year}",
                userId, month, year);

            var timesheet = await _repository.GetMonthlyTimesheetAsync(userId, month, year);
            timesheet.EmployeeName = $"Employee {userId}";

            return timesheet;
        }

        public async Task<StationShiftStatisticsDTO> GetStationStatisticsAsync(int stationId, DateTime fromDate, DateTime toDate)
        {
            _logger.LogInformation("Getting statistics for Station {StationId}", stationId);

            var statistics = await _repository.GetStationStatisticsAsync(stationId, fromDate, toDate);
            statistics.StationName = $"Station {stationId}";

            return statistics;
        }

        // ==================== Helper Methods ====================

        private StaffShiftResponseDTO MapToResponseDTO(StaffShift shift)
        {
            return new StaffShiftResponseDTO
            {
                Id = shift.Id,
                UserId = shift.UserId,
                EmployeeName = null, // TODO: Fetch from UserService
                StationId = shift.StationId,
                StationName = shift.Station?.Name,
                ShiftDate = shift.ShiftDate,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime,
                Status = shift.Status,
                ActualCheckInTime = shift.ActualCheckInTime,
                ActualCheckOutTime = shift.ActualCheckOutTime,
                ScheduledDuration = shift.ScheduledDuration,
                ActualDuration = shift.ActualDuration,
                Notes = shift.Notes,
                CreatedAt = shift.CreatedAt
            };
        }
    }
}