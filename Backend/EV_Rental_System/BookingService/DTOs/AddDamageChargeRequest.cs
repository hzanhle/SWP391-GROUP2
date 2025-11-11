using System.ComponentModel.DataAnnotations;

namespace BookingService.DTOs
{
    /// <summary>
    /// Request to add damage charge to a settlement
    /// </summary>
    public class AddDamageChargeRequest
    {
        /// <summary>
        /// Amount of damage charge in VND
        /// </summary>
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Damage charge must be greater than 0")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Description of the damage
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }
    }
}
