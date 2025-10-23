namespace StationService.DTOs.StaffShift
{
    public class MonthlyTimesheetDTO
    {
        public int UserId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public int TotalShifts { get; set; }
        public int CompletedShifts { get; set; }
        public double TotalHoursScheduled { get; set; }
        public double TotalHoursWorked { get; set; }
        public List<ShiftSummaryDTO> Shifts { get; set; } = new();
    }

    public class ShiftSummaryDTO
    {
        public int ShiftId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public double HoursWorked { get; set; }
    }
}
