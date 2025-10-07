namespace UserService.DTOs
{
    public class OtpResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Email { get; set; }
        public string Otp { get; set; }
    }
}
