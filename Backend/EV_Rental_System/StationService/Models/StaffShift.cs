namespace StationService.Models
{
    public class StaffShift
    {
        public int Id { get; set; }
        public int UserId { get; set; }  // ✅ Chỉ lưu FK, không có User navigation

        public int StationId { get; set; }
        public Station Station { get; set; }  // ✅ Trong cùng module thì có

        public DateOnly ShiftDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
