namespace BookingSerivce.DTOs
{
    public class OrderRequest
    {
        public int UserId { get; set; }
        public int VehicleId { get; set; }
        public decimal ModelPrice { get; set; } // Giá của model xe ( tính phí đặt cọc)
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalTime { get; set; }
        public decimal TotalCost { get; set; } // Tổng chi phí thuê xe ( tính phí thuê xe)
    }
}
