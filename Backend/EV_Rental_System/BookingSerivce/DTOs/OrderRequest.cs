namespace BookingSerivce.DTOs
{
    public class OrderRequest
    {
        public int UserId { get; set; }
        public int VehicleId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalTime { get; set; }
        
    }
}
