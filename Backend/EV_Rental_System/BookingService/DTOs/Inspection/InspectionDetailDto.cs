namespace BookingService.DTOs.Inspection
{
    /// <summary>
    /// Detailed inspection information including damages and photos
    /// </summary>
    public class InspectionDetailDto
    {
        public int InspectionId { get; set; }
        public int OrderId { get; set; }
        public int VehicleId { get; set; }
        public string InspectionType { get; set; }
        public int InspectorUserId { get; set; }
        public DateTime InspectionDate { get; set; }
        public int? Mileage { get; set; }
        public int? BatteryLevel { get; set; }
        public string? OverallCondition { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<DamageDto> Damages { get; set; } = new();
        public List<PhotoDto> Photos { get; set; } = new();
    }

    public class PhotoDto
    {
        public int PhotoId { get; set; }
        public string PhotoUrl { get; set; }
        public string? PhotoType { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
