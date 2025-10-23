namespace StationService.DTOs.StaffShift
{
    public class CreateStaffShiftDTO
    {
        public int UserId { get; set; }
        public int StationId { get; set; }
        public DateTime ShiftDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Notes { get; set; }
    }
}
