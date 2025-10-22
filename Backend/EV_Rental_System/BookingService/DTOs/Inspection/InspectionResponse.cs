using BookingService.Models.Enums;

namespace BookingService.DTOs.Inspection
{
    /// <summary>
    /// Response containing inspection details
    /// </summary>
    public class InspectionResponse
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
        public int DamageCount { get; set; }
        public int PhotoCount { get; set; }
    }
}
