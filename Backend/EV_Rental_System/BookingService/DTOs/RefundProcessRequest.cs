namespace BookingService.DTOs
{
    /// <summary>
    /// Request for processing refund (admin marks refund as processed/failed)
    /// </summary>
    public class RefundProcessRequest
    {
        /// <summary>
        /// Admin's notes about the refund process
        /// </summary>
        public string? Notes { get; set; }
    }
}
