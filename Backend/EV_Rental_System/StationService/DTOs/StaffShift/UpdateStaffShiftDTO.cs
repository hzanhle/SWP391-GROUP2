namespace StationService.DTOs.StaffShift
{
    public class UpdateStaffShiftDTO
    {
        public int? UserId { get; set; }
        public DateTime? ShiftDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
    }
}
