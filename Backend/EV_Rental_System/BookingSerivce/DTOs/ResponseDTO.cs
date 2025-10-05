namespace BookingSerivce.DTOs
{
    public class ResponseDTO
    {
        public bool? IsSuccess { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }        
    }
}
