namespace StationService.DTOs.StaffShift
{
    public class ShiftSwapRequestDTO
    {
        public int MyShiftId { get; set; }
        public int TargetShiftId { get; set; }
        public string? Reason { get; set; }
    }
}
