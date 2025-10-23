using System.ComponentModel.DataAnnotations;

namespace StationService.Models
{
    public class StaffShift
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; } // Employee ID

        [Required]
        public int StationId { get; set; }
        public Station? Station { get; set; }

        [Required]
        public DateTime ShiftDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        // Trạng thái ca: Scheduled, Confirmed, CheckedIn, Completed, Cancelled, NoShow
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Scheduled";

        // Check-in/Check-out
        public DateTime? ActualCheckInTime { get; set; }
        public DateTime? ActualCheckOutTime { get; set; }

        // Ghi chú từ Admin
        [StringLength(500)]
        public string? Notes { get; set; }

        // Lý do hủy/từ chối
        [StringLength(500)]
        public string? CancellationReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Phương thức tính toán thời lượng ca làm việc
        public TimeSpan ScheduledDuration => EndTime - StartTime;

        public TimeSpan? ActualDuration
        {
            get
            {
                if (ActualCheckInTime.HasValue && ActualCheckOutTime.HasValue)
                {
                    return ActualCheckOutTime.Value - ActualCheckInTime.Value;
                }
                return null;
            }
        }

        // Logic để kiểm tra tính hợp lệ của thời lượng ca làm việc
        public bool IsValidShiftDuration()
        {
            var duration = ScheduledDuration;
            return duration.TotalHours >= 6 && duration.TotalHours <= 8;
        }
    }
}