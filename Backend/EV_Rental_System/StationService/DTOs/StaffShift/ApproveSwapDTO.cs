namespace StationService.DTOs.StaffShift
{
    public class ApproveSwapDTO
    {
        public int Shift1Id { get; set; }
        public int Shift2Id { get; set; }
        public bool IsApproved { get; set; }
        public string? RejectionReason { get; set; }
    }
}
