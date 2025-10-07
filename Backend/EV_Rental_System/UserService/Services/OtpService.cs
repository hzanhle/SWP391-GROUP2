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
        private readonly IUserRepository _userRepository; // ✨ THÊM dependency này

        public OtpService(
            IOptions<EmailSettings> emailSettings,
            IDistributedCache cache,
            IUserRepository userRepository) // ✨ THÊM vào constructor
        {
            _emailSettings = emailSettings.Value;
            _cache = cache;
            _userRepository = userRepository; // ✨ THÊM
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

        public async Task<OtpResponse> SendPasswordResetOtpAsync(string email)
        {
            try
            {
                // Kiểm tra email có tồn tại không
                var user = await _userRepository.GetUserByEmailAsync(email);
                if (user == null)
                {
                    // Không tiết lộ email không tồn tại (security)
                    return new OtpResponse
                    {
                        Success = true,
                        Message = "Nếu email tồn tại, mã OTP đã được gửi đến hộp thư của bạn"
                    };
                }

                // Tạo OTP 6 số
                var otp = GenerateOtp();

                // Gửi email
                var sent = await SendPasswordResetEmailAsync(email, otp);

                if (!sent)
                {
                    return new OtpResponse
                    {
                        Success = false,
                        Message = "Không thể gửi email. Vui lòng thử lại sau."
                    };
                }

                // Lưu OTP với prefix RESET_ để phân biệt
                await StoreOtpAsync($"RESET_{email}", otp);

                return new OtpResponse
                {
                    Success = true,
                    Message = "Mã OTP đã được gửi đến email của bạn. Vui lòng kiểm tra hộp thư."
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
        public async Task<OtpResponse> VerifyPasswordResetOtpAsync(string email, string otp)
        {
            try
            {
                // Lấy OTP từ cache với prefix RESET_
                var cacheKey = $"OTP_RESET_{email}";
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
                    // KHÔNG xóa OTP ở đây - sẽ xóa sau khi reset password thành công
                    return new OtpResponse
                    {
                        Success = true,
                        Message = "Xác thực thành công. Bạn có thể đặt mật khẩu mới.",
                        Email = email,
                        Otp = otp // Giữ lại để dùng cho bước reset
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

        // ========== PRIVATE METHODS ==========

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
                using var mail = new MailMessage();
                mail.From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName);
                mail.To.Add(email);
                mail.Subject = "Mã OTP xác thực";
                mail.Body = CreateEmailBody(otp, "Mã OTP của bạn");
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

        // ✨ NEW - Email cho reset password
        private async Task<bool> SendPasswordResetEmailAsync(string email, string otp)
        {
            try
            {
                using var mail = new MailMessage();
                mail.From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName);
                mail.To.Add(email);
                mail.Subject = "Đặt lại mật khẩu - Mã OTP";
                mail.Body = CreateEmailBody(otp, "Đặt lại mật khẩu");
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
