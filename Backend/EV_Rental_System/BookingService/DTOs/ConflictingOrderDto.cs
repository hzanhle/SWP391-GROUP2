namespace BookingService.DTOs
{
    /// <summary>
    /// DTO for orders that conflict with a requested date range
    /// Used by availability checking system
    /// </summary>
    public class ConflictingOrderDto
    {
        public int OrderId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Status { get; set; }
    }
}
