namespace BookingService.DTOs
{
    /// <summary>
    /// Request for manual refund with proof document upload
    /// </summary>
    public class ManualRefundRequest
    {
        /// <summary>
        /// Proof document (screenshot from VNPay portal, bank transfer receipt, etc.)
        /// Allowed formats: .jpg, .jpeg, .png, .pdf
        /// Max size: 5MB
        /// </summary>
        public IFormFile ProofDocument { get; set; } = null!;

        /// <summary>
        /// Optional notes about the manual refund process
        /// </summary>
        public string? Notes { get; set; }
    }
}
