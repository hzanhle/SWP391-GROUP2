using BookingService.Models.Enums;

namespace BookingService.DTOs.Fees
{
    /// <summary>
    /// Data transfer object for additional fees
    /// </summary>
    public class FeeDto
    {
        public int FeeId { get; set; }
        public int OrderId { get; set; }
        public FeeType FeeType { get; set; }
        public string FeeTypeName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public int? CalculatedBy { get; set; }
        public bool IsPaid { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
