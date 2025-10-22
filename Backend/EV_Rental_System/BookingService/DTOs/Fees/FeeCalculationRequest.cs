using System.ComponentModel.DataAnnotations;

namespace BookingService.DTOs.Fees
{
    /// <summary>
    /// Request for calculating fees after rental completion
    /// </summary>
    public class FeeCalculationRequest
    {
        [Required]
        public int OrderId { get; set; }

        /// <summary>
        /// Actual return date/time (for late return calculation)
        /// </summary>
        [Required]
        public DateTime ActualReturnDate { get; set; }

        /// <summary>
        /// Vehicle mileage at return (for excess mileage calculation)
        /// </summary>
        public int? ReturnMileage { get; set; }

        /// <summary>
        /// Pickup mileage (for calculating distance traveled)
        /// </summary>
        public int? PickupMileage { get; set; }

        /// <summary>
        /// Optional: specific fee types to calculate
        /// If null/empty, all applicable fees will be calculated
        /// </summary>
        public List<string>? FeeTypesToCalculate { get; set; }
    }
}
