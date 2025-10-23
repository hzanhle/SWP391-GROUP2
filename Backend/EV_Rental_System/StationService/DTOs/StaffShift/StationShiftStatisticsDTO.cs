namespace StationService.DTOs.StaffShift
{
    public class StationShiftStatisticsDTO
    {
        public int StationId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalShifts { get; set; }
        public int CompletedShifts { get; set; }
        public int CancelledShifts { get; set; }
        public int NoShowShifts { get; set; }
        public double AverageCheckInDelay { get; set; }
        public List<EmployeeShiftSummary> EmployeeSummaries { get; set; } = new();
    }

    public class EmployeeShiftSummary
    {
        public int UserId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public int TotalShifts { get; set; }
        public int CompletedShifts { get; set; }
        public double TotalHours { get; set; }
    }
}
