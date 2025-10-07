using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using UserService.DTOs;
using UserService.Models;

namespace UserService.Services
{
    public class OtpService : IOtpService
    {
        private readonly EmailSettings _emailSettings;
        private readonly IDistributedCache _cache;

        public OtpService(IOptions<EmailSettings> emailSettings, IDistributedCache cache)
        {
            _emailSettings = emailSettings.Value;
            _cache = cache;
        }

        // ✅ Gửi OTP đến email
        public async Task<OtpResponse> SendOtpAsync(string email)
        {
            try
            {
                // Tạo OTP 6 số
                var otp = GenerateOtp();

                // Gửi email
                var sent = await SendEmailAsync(email, otp);

                if (!sent)
                {
                    return new OtpResponse
                    {
                        Success = false,
                        Message = "Không thể gửi email"
                    };
                }

                // Lưu OTP vào cache (5 phút)
                await StoreOtpAsync(email, otp);

                return new OtpResponse
                {
                    Success = true,
                    Message = "OTP đã được gửi đến email của bạn"
                };
            }
            catch (Exception ex)
            {
                return new OtpResponse
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        // ✅ Xác thực OTP
        public async Task<OtpResponse> VerifyOtpAsync(string email, string otp)
        {
            try
            {
                // Lấy OTP từ cache
                var cacheKey = $"OTP_{email}";
                var cachedOtp = await _cache.GetStringAsync(cacheKey);

                if (string.IsNullOrEmpty(cachedOtp))
                {
                    return new OtpResponse
                    {
                        Success = false,
                        Message = "OTP không tồn tại hoặc đã hết hạn"
                    };
                }

                // So sánh OTP
                if (cachedOtp == otp)
                {
                    // Xóa OTP sau khi verify thành công
                    await _cache.RemoveAsync(cacheKey);

                    return new OtpResponse
                    {
                        Success = true,
                        Message = "Xác thực thành công"
                    };
                }

                return new OtpResponse
                {
                    Success = false,
                    Message = "OTP không chính xác"
                };
            }
            catch (Exception ex)
            {
                return new OtpResponse
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        // ============================================
        // PRIVATE METHODS
        // ============================================

        // Tạo OTP 6 số ngẫu nhiên
        private string GenerateOtp()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[6];
            rng.GetBytes(bytes);

            var otp = "";
            foreach (var b in bytes)
            {
                otp += (b % 10).ToString();
            }
            return otp;
        }

        // Lưu OTP vào cache
        private async Task StoreOtpAsync(string email, string otp)
        {
            var cacheKey = $"OTP_{email}";
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };

            await _cache.SetStringAsync(cacheKey, otp, options);
        }

        // Gửi email
        private async Task<bool> SendEmailAsync(string email, string otp)
        {
            try
            {
                using var mail = new MailMessage();
                mail.From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName);
                mail.To.Add(email);
                mail.Subject = "Mã OTP xác thực";
                mail.Body = CreateEmailBody(otp);
                mail.IsBodyHtml = true;

                using var smtp = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort);
                smtp.Credentials = new NetworkCredential(
                    _emailSettings.SenderEmail,
                    _emailSettings.SenderPassword);
                smtp.EnableSsl = _emailSettings.EnableSsl;

                await smtp.SendMailAsync(mail);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Tạo nội dung email
        private string CreateEmailBody(string otp)
        {
            return $@"
<!DOCTYPE html>
<html>
<body style='font-family: Arial; padding: 20px;'>
    <div style='max-width: 600px; margin: 0 auto; background: white; padding: 30px; border-radius: 10px;'>
        <h2 style='color: #333; text-align: center;'>Mã OTP của bạn</h2>
        <div style='text-align: center; margin: 30px 0;'>
            <span style='font-size: 32px; font-weight: bold; color: #4CAF50; letter-spacing: 5px;'>
                {otp}
            </span>
        </div>
        <p style='color: #666; text-align: center;'>
            Mã OTP có hiệu lực trong <strong>5 phút</strong>
        </p>
    </div>
</body>
</html>";
        }
    }
}
