using System.ComponentModel.DataAnnotations;

namespace BookingService.DTOs
{
    public class OrderRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Model price must be greater than 0")]
        public decimal ModelPrice { get; set; } // Giá của model (FE lấy từ API)

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Rent fee must be greater than 0")]
        public decimal RentFeeForHour { get; set; } // Giá thuê/giờ (FE lấy từ API)

        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public DateTime ToDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total rental cost must be greater than 0")]
        public decimal TotalRentalCost { get; set; } // FE đã tính = hours * RentFeeForHour

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Deposit amount must be greater than 0")]
        public decimal DepositAmount { get; set; } // FE đã tính = ModelPrice * 0.3

    }
}
