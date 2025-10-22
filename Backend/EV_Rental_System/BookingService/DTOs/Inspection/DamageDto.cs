namespace BookingService.DTOs.Inspection
{
    /// <summary>
    /// DTO representing a damage record
    /// </summary>
    public class DamageDto
    {
        public int DamageId { get; set; }
        public int InspectionId { get; set; }
        public string DamageType { get; set; }
        public string? Location { get; set; }
        public string Severity { get; set; }
        public string? Description { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string? PhotoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
