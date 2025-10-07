namespace UserService.DTOs
{
    public class OtpResponse
    {
        public string Email { get; set; }
        public string Message { get; set; }
        public string Otp { get; set; }
        public bool Success { get; set; }
    }
}
