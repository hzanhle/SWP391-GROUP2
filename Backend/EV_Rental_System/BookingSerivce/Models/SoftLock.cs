using System.ComponentModel.DataAnnotations;

namespace BookingSerivce.Models
{
    /// <summary>
    /// Represents a temporary reservation lock on a vehicle during the preview-to-confirm window.
    /// Prevents double-booking race conditions by reserving the vehicle for 5 minutes.
    /// </summary>
    public class SoftLock
    {
        [Key]
        public Guid LockToken { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public DateTime ToDate { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime ExpiresAt { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // Active, Consumed, Expired

        /// <summary>
        /// Checks if this soft lock is still valid (Active and not expired)
        /// </summary>
        public bool IsValid()
        {
            return Status == "Active" && ExpiresAt > DateTime.UtcNow;
        }

        /// <summary>
        /// Marks this soft lock as consumed (converted to an order)
        /// </summary>
        public void Consume()
        {
            Status = "Consumed";
        }

        /// <summary>
        /// Marks this soft lock as expired
        /// </summary>
        public void Expire()
        {
            Status = "Expired";
        }
    }
}
