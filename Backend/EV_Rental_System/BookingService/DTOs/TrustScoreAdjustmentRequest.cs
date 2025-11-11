using System.ComponentModel.DataAnnotations;

namespace BookingService.DTOs
{
    /// <summary>
    /// Request for admin to manually adjust a user's trust score
    /// </summary>
    public class TrustScoreAdjustmentRequest
    {
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Points to add or subtract (can be positive or negative)
        /// Example: +10, -20
        /// </summary>
        [Required]
        [Range(-200, 200, ErrorMessage = "Adjustment must be between -200 and +200")]
        public int ChangeAmount { get; set; }

        /// <summary>
        /// Required reason for the adjustment
        /// Example: "Appeal approved", "Correction for system error", "Penalty for violation"
        /// </summary>
        [Required]
        [MinLength(10, ErrorMessage = "Reason must be at least 10 characters")]
        [MaxLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// ID of the admin making the adjustment (set from JWT token in controller)
        /// </summary>
        public int AdminId { get; set; }
    }
}
