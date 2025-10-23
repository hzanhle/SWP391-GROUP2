using StationService.DTOs.StaffShift;
using StationService.Models;

namespace StationService.Services
{
    public interface IStaffShiftService
    {
        Task<StaffShiftResponseDTO?> GetShiftByIdAsync(int id);
        Task<List<StaffShiftResponseDTO>> GetAllShiftsAsync();
        Task<StaffShiftResponseDTO> CreateShiftAsync(CreateStaffShiftDTO dto);
        Task<StaffShiftResponseDTO> UpdateShiftAsync(int id, UpdateStaffShiftDTO dto);
        Task<bool> DeleteShiftAsync(int id);

        Task<List<StaffShiftResponseDTO>> GetShiftsByUserIdAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<StaffShiftResponseDTO>> GetShiftsByStationIdAsync(int stationId, DateTime? fromDate = null, DateTime? toDate = null);

        Task<bool> CheckInAsync(CheckInOutDTO dto);
        Task<bool> CheckOutAsync(CheckInOutDTO dto);

        Task<bool> SwapShiftsAsync(int shift1Id, int shift2Id, int requestingUserId);

        Task<MonthlyTimesheetDTO> GetMonthlyTimesheetAsync(int userId, int month, int year);
        Task<StationShiftStatisticsDTO> GetStationStatisticsAsync(int stationId, DateTime fromDate, DateTime toDate);
    }
}
