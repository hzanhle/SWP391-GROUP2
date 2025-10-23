namespace StationService.DTOs.StaffShift
{
    public class StaffShiftResponseDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? EmployeeName { get; set; }
        public int StationId { get; set; }
        public string? StationName { get; set; }
        public DateTime ShiftDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ActualCheckInTime { get; set; }
        public DateTime? ActualCheckOutTime { get; set; }
        public TimeSpan ScheduledDuration { get; set; }
        public TimeSpan? ActualDuration { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
