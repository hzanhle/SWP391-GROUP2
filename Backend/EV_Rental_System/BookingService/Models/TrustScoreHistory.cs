using System.ComponentModel.DataAnnotations;

namespace BookingService.Models
{
    /// <summary>
    /// Tracks all changes to a user's trust score with reasons
    /// </summary>
    public class TrustScoreHistory
    {
        [Key]
        public int HistoryId { get; set; }

        /// <summary>
        /// User whose score changed
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Related order (if applicable)
        /// </summary>
        public int? OrderId { get; set; }

        /// <summary>
        /// Points added or subtracted (positive or negative)
        /// </summary>
        public int ChangeAmount { get; set; }

        /// <summary>
        /// Score value before this change
        /// </summary>
        public int PreviousScore { get; set; }

        /// <summary>
        /// Score value after this change
        /// </summary>
        public int NewScore { get; set; }

        /// <summary>
        /// Reason for the score change
        /// Examples: "First payment bonus", "Rental completion", "Late return penalty", "Admin adjustment: Appeal approved"
        /// </summary>
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Type of change for filtering/reporting
        /// Examples: "Bonus", "Penalty", "ManualAdjustment"
        /// </summary>
        [MaxLength(50)]
        public string ChangeType { get; set; } = string.Empty;

        /// <summary>
        /// Admin who made manual adjustment (null for automatic changes)
        /// </summary>
        public int? AdjustedByAdminId { get; set; }

        /// <summary>
        /// When the change occurred
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
