using System.ComponentModel.DataAnnotations;

namespace BookingService.DTOs
{
    /// <summary>
    /// Request to calculate settlement for an order
    /// </summary>
    public class SettlementCalculationRequest
    {
        [Required]
        public int OrderId { get; set; }

        /// <summary>
        /// Actual time vehicle was returned
        /// </summary>
        [Required]
        public DateTime ActualReturnTime { get; set; }
    }
}
