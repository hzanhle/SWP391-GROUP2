using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BookingService.Models.Enums;

namespace BookingService.Models
{
    /// <summary>
    /// Additional fees charged to customer after rental
    /// (late return, damage, excess mileage, etc.)
    /// </summary>
    public class AdditionalFee
    {
        [Key]
        public int FeeId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FeeType FeeType { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// User ID of staff who added/calculated the fee
        /// </summary>
        public int? CalculatedBy { get; set; }

        /// <summary>
        /// Whether the fee has been paid by customer
        /// </summary>
        public bool IsPaid { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey(nameof(OrderId))]
        public Order? Order { get; set; }
    }
}
