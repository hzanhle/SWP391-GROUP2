using System.ComponentModel.DataAnnotations;

namespace BookingSerivce.DTOs
{
    public class OrderRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [Required]
        public decimal ModelPrice { get; set; } // Giá của model xe (tính phí đặt cọc)

        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public DateTime ToDate { get; set; }

        [Required]
        public int TotalTime { get; set; } // Total hours or days

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total cost must be greater than 0")]
        public decimal TotalCost { get; set; } // Tổng chi phí thuê xe
    }
}
