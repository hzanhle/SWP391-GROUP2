namespace BookingSerivce.DTOs
{
    public class OnlineContractDTO
    {
        public int ContractId { get; set; }
        public int OrderId { get; set; }
        public string Terms { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SignedAt { get; set; } // Nullable to indicate it may not be signed yet
        public string Status { get; set; } // e.g., "Draft", "Sent", "Signed", "Cancelled"
    }
}
