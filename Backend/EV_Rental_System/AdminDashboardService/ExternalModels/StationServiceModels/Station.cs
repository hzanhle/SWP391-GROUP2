using System.ComponentModel.DataAnnotations;

namespace AdminDashboardService.ExternalModels.StationServiceModels
{
    public class Station
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int? ManagerId { get; set; }
        public bool IsActive { get; set; }
    }

    public class StaffShift
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int StationId { get; set; }
        public DateTime ShiftDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class Feedback
    {
        [Key]
        public int FeedbackId { get; set; }
        public int StationId { get; set; }
        public int UserId { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}