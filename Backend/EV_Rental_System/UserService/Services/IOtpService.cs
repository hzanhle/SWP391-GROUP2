using UserService.DTOs;

namespace UserService.Services
{
    public interface IOtpService
    {
        Task<OtpResponse> SendOtpAsync(string email);
        Task<OtpResponse> VerifyOtpAsync(string email, string otp);
        Task<OtpResponse> SendPasswordResetOtpAsync(string email);
        Task<OtpResponse> VerifyPasswordResetOtpAsync(string email, string otp);
    }
}
