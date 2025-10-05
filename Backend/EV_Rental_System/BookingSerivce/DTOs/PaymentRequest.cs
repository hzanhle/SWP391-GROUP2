namespace BookingSerivce.DTOs
{
    public class PaymentRequest
    {
        public int OrderId { get; set; }
        public string PaymentMethod { get; set; }
    }
}
