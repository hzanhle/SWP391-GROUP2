using System.ComponentModel.DataAnnotations;
using BookingService.Models.Enums;

namespace BookingService.DTOs.Fees
{
    /// <summary>
    /// Request for manually adding a fee to an order
    /// </summary>
    public class AddFeeRequest
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public FeeType FeeType { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// User ID of staff member adding the fee (will be set from JWT token)
        /// </summary>
        public int? CalculatedBy { get; set; }
    }
}
