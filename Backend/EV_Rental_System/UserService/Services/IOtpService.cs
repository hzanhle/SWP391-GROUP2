using UserService.DTOs;

namespace UserService.Services
{
    public interface IOtpService
    {
        Task<OtpAttribute> SendOtpAsync(string email);
        Task<OtpAttribute> VerifyOtpAsync(string email, string otp);
        Task<OtpAttribute> SendPasswordResetOtpAsync(string email);
        Task<OtpAttribute> VerifyPasswordResetOtpAsync(string email, string otp);
    }
}
