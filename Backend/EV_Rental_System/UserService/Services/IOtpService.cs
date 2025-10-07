using UserService.DTOs;

namespace UserService.Services
{
    public interface IOtpService
    {
        Task<OtpResponse> SendOtpAsync(string email);
        Task<OtpResponse> VerifyOtpAsync(string email, string otp);
    }
}
