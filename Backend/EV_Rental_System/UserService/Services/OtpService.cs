using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using UserService.DTOs;
using UserService.Models;
using UserService.Repositories;

namespace UserService.Services
{
    public class OtpService : IOtpService
    {
        private readonly EmailSettings _emailSettings;
        private readonly IDistributedCache _cache;
        private readonly IUserRepository _userRepository;

        public OtpService(
            IOptions<EmailSettings> emailSettings,
            IDistributedCache cache,
            IUserRepository userRepository)
        {
            _emailSettings = emailSettings.Value;
            _cache = cache;
            _userRepository = userRepository;
        }

        // ✅ Gửi OTP đến email
        public async Task<OtpResponse> SendOtpAsync(string email)
        {
            try
            {
                var otp = GenerateOtp();
                var sent = await SendEmailAsync(email, otp);

                if (!sent)
                {
                    return new OtpResponse { Success = false, Message = "Không thể gửi email" };
                }

                await StoreOtpAsync(email, otp);

                return new OtpResponse { Success = true, Message = "OTP đã được gửi đến email của bạn" };
            }
            catch (Exception ex)
            {
                return new OtpResponse { Success = false, Message = $"Lỗi: {ex.Message}" };
            }
        }

        // ✅ Xác thực OTP
        public async Task<OtpResponse> VerifyOtpAsync(string email, string otp)
        {
            try
            {
                var cacheKey = $"OTP_{email}";
                var cachedOtp = await _cache.GetStringAsync(cacheKey);

                if (string.IsNullOrEmpty(cachedOtp))
                    return new OtpResponse { Success = false, Message = "OTP không tồn tại hoặc đã hết hạn" };

                if (cachedOtp == otp)
                {
                    await _cache.RemoveAsync(cacheKey);
                    return new OtpResponse { Success = true, Message = "Xác thực thành công" };
                }

                return new OtpResponse { Success = false, Message = "OTP không chính xác" };
            }
            catch (Exception ex)
            {
                return new OtpResponse { Success = false, Message = $"Lỗi: {ex.Message}" };
            }
        }

        // ✅ Gửi OTP cho đặt lại mật khẩu
        public async Task<OtpResponse> SendPasswordResetOtpAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetUserByEmailAsync(email);
                if (user == null)
                {
                    // Không tiết lộ thông tin nhạy cảm
                    return new OtpResponse
                    {
                        Success = true,
                        Message = "Nếu email tồn tại, mã OTP đã được gửi đến hộp thư của bạn"
                    };
                }

                var otp = GenerateOtp();
                var sent = await SendPasswordResetEmailAsync(email, otp);

                if (!sent)
                    return new OtpResponse { Success = false, Message = "Không thể gửi email. Vui lòng thử lại sau." };

                await StoreOtpAsync($"RESET_{email}", otp);

                return new OtpResponse
                {
                    Success = true,
                    Message = "Mã OTP đã được gửi đến email của bạn. Vui lòng kiểm tra hộp thư."
                };
            }
            catch (Exception ex)
            {
                return new OtpResponse { Success = false, Message = $"Lỗi: {ex.Message}" };
            }
        }

        // ✅ Xác thực OTP cho đặt lại mật khẩu
        public async Task<OtpResponse> VerifyPasswordResetOtpAsync(string email, string otp)
        {
            try
            {
                var cacheKey = $"OTP_RESET_{email}";
                var cachedOtp = await _cache.GetStringAsync(cacheKey);

                if (string.IsNullOrEmpty(cachedOtp))
                    return new OtpResponse { Success = false, Message = "OTP không tồn tại hoặc đã hết hạn" };

                if (cachedOtp == otp)
                {
                    return new OtpResponse
                    {
                        Success = true,
                        Message = "Xác thực thành công. Bạn có thể đặt mật khẩu mới.",
                        Email = email,
                        Otp = otp
                    };
                }

                return new OtpResponse { Success = false, Message = "OTP không chính xác" };
            }
            catch (Exception ex)
            {
                return new OtpResponse { Success = false, Message = $"Lỗi: {ex.Message}" };
            }
        }

        // ========== PRIVATE METHODS ==========

        private string GenerateOtp()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[6];
            rng.GetBytes(bytes);
            return string.Concat(bytes.Select(b => (b % 10).ToString()));
        }

        private async Task StoreOtpAsync(string key, string otp)
        {
            var cacheKey = $"OTP_{key}";
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            await _cache.SetStringAsync(cacheKey, otp, options);
        }

        private async Task<bool> SendEmailAsync(string email, string otp)
        {
            try
            {
                using var mail = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = "Mã OTP xác thực",
                    Body = CreateEmailBody(otp, "Mã OTP của bạn"),
                    IsBodyHtml = true
                };
                mail.To.Add(email);

                using var smtp = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.SenderPassword),
                    EnableSsl = _emailSettings.EnableSsl
                };

                await smtp.SendMailAsync(mail);
                return true;
            }
            catch { return false; }
        }

        private async Task<bool> SendPasswordResetEmailAsync(string email, string otp)
        {
            try
            {
                using var mail = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = "Đặt lại mật khẩu - Mã OTP",
                    Body = CreateEmailBody(otp, "Đặt lại mật khẩu"),
                    IsBodyHtml = true
                };
                mail.To.Add(email);

                using var smtp = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.SenderPassword),
                    EnableSsl = _emailSettings.EnableSsl
                };

                await smtp.SendMailAsync(mail);
                return true;
            }
            catch { return false; }
        }

        private string CreateEmailBody(string otp, string title)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <body style='font-family: Arial; padding: 20px;'>
                <div style='max-width: 600px; margin: 0 auto; background: white; padding: 30px; border-radius: 10px;'>
                    <h2 style='color: #333; text-align: center;'>{title}</h2>
                    <div style='text-align: center; margin: 30px 0;'>
                        <span style='font-size: 32px; font-weight: bold; color: #4CAF50; letter-spacing: 5px;'>
                            {otp}
                        </span>
                    </div>
                    <p style='color: #666; text-align: center;'>
                        Mã OTP có hiệu lực trong <strong>5 phút</strong>
                    </p>
                    <p style='color: #999; font-size: 12px; text-align: center;'>
                        Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email này.
                    </p>
                </div>
            </body>
            </html>";
        }
    }
}
