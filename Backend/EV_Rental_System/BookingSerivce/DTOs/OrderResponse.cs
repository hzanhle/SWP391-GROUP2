namespace BookingSerivce.DTOs
{
    public class OrderResponse
    {
        public int OrderId { get; set; }
        public string Status { get; set; }
        public string ContractNumber { get; set; }
        public DateTime? ContractExpiresAt { get; set; }
        public decimal TotalAmount { get; set; } // TotalCost + Deposit + ServiceFee
        public string Message { get; set; }
    }

}
