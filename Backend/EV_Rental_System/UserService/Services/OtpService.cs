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
        private readonly ILogger<OtpService> _logger;

        public OtpService(
            IOptions<EmailSettings> emailSettings,
            IDistributedCache cache,
            IUserRepository userRepository,
            ILogger<OtpService> logger)
        {
            _emailSettings = emailSettings.Value;
            _cache = cache;
            _userRepository = userRepository;
            _logger = logger;
        }

        // ✅ Gửi OTP đến email
        public async Task<OtpAttribute> SendOtpAsync(string email)
        {
            try
            {
                var otp = GenerateOtp();
                var sent = await SendEmailAsync(email, otp);

                if (!sent)
                {
                    return new OtpAttribute { Success = false, Message = "Không thể gửi email" };
                }

                await StoreOtpAsync(email, otp);

                return new OtpAttribute { Success = true, Message = "OTP đã được gửi đến email của bạn" };
            }
            catch (Exception ex)
            {
                return new OtpAttribute { Success = false, Message = $"Lỗi: {ex.Message}" };
            }
        }

        // ✅ Xác thực OTP
        public async Task<OtpAttribute> VerifyOtpAsync(string email, string otp)
        {
            try
            {
                var cacheKey = $"OTP_{email}";
                var cachedOtp = await _cache.GetStringAsync(cacheKey);

                if (string.IsNullOrEmpty(cachedOtp))
                    return new OtpAttribute { Success = false, Message = "OTP không tồn tại hoặc đã hết hạn" };

                if (cachedOtp == otp)
                {
                    await _cache.RemoveAsync(cacheKey);
                    return new OtpAttribute { Success = true, Message = "Xác thực thành công" };
                }

                return new OtpAttribute { Success = false, Message = "OTP không chính xác" };
            }
            catch (Exception ex)
            {
                return new OtpAttribute { Success = false, Message = $"Lỗi: {ex.Message}" };
            }
        }

        // ✅ Gửi OTP cho đặt lại mật khẩu
        public async Task<OtpAttribute> SendPasswordResetOtpAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetUserByEmailAsync(email);
                if (user == null)
                {
                    // Không tiết lộ thông tin nhạy cảm
                    return new OtpAttribute
                    {
                        Success = true,
                        Message = "Nếu email tồn tại, mã OTP đã được gửi đến hộp thư của bạn"
                    };
                }

                var otp = GenerateOtp();
                var sent = await SendPasswordResetEmailAsync(email, otp);

                if (!sent)
                    return new OtpAttribute { Success = false, Message = "Không thể gửi email. Vui lòng thử lại sau." };

                await StoreOtpAsync($"RESET_{email}", otp);

                return new OtpAttribute
                {
                    Success = true,
                    Message = "Mã OTP đã được gửi đến email của bạn. Vui lòng kiểm tra hộp thư."
                };
            }
            catch (Exception ex)
            {
                return new OtpAttribute { Success = false, Message = $"Lỗi: {ex.Message}" };
            }
        }

        // ✅ Xác thực OTP cho đặt lại mật khẩu
        public async Task<OtpAttribute> VerifyPasswordResetOtpAsync(string email, string otp)
        {
            try
            {
                var cacheKey = $"OTP_RESET_{email}";
                var cachedOtp = await _cache.GetStringAsync(cacheKey);

                if (string.IsNullOrEmpty(cachedOtp))
                    return new OtpAttribute { Success = false, Message = "OTP không tồn tại hoặc đã hết hạn" };

                if (cachedOtp == otp)
                {
                    return new OtpAttribute
                    {
                        Success = true,
                        Message = "Xác thực thành công. Bạn có thể đặt mật khẩu mới.",
                        Email = email,
                        Otp = otp
                    };
                }

                return new OtpAttribute { Success = false, Message = "OTP không chính xác" };
            }
            catch (Exception ex)
            {
                return new OtpAttribute { Success = false, Message = $"Lỗi: {ex.Message}" };
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
                _logger.LogInformation("Bắt đầu gửi email đặt lại mật khẩu đến {Email}", email);

                using var mail = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = "Đặt lại mật khẩu - Mã OTP",
                    Body = CreateEmailBody(otp, "Đặt lại mật khẩu"),
                    IsBodyHtml = true
                };
                mail.To.Add(email);

                _logger.LogDebug("Đã tạo MailMessage với Subject: {Subject}", mail.Subject);

                using var smtp = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.SenderPassword),
                    EnableSsl = _emailSettings.EnableSsl
                };

                _logger.LogDebug("Kết nối SMTP Server: {Server}:{Port}, SSL: {EnableSsl}",
                    _emailSettings.SmtpServer, _emailSettings.SmtpPort, _emailSettings.EnableSsl);

                await smtp.SendMailAsync(mail);

                _logger.LogInformation("Đã gửi email đặt lại mật khẩu thành công đến {Email}", email);
                return true;
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "Lỗi SMTP khi gửi email đặt lại mật khẩu đến {Email}. StatusCode: {StatusCode}",
                    email, ex.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi không xác định khi gửi email đặt lại mật khẩu đến {Email}", email);
                return false;
            }
        }

        private string CreateRegistrationEmailBody(string otp, string userName)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <body style='font-family: Arial; padding: 20px;'>
                <div style='max-width: 600px; margin: 0 auto; background: white; padding: 30px; border-radius: 10px;'>
                    <h2 style='color: #333; text-align: center;'>Xác thực đăng ký tài khoản</h2>
                    <p style='color: #666; text-align: center;'>
                        Xin chào <strong>{userName}</strong>,
                    </p>
                    <p style='color: #666; text-align: center;'>
                        Cảm ơn bạn đã đăng ký tài khoản. Vui lòng sử dụng mã OTP bên dưới để hoàn tất đăng ký:
                    </p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <span style='font-size: 32px; font-weight: bold; color: #4CAF50; letter-spacing: 5px;'>
                            {otp}
                        </span>
                    </div>
                    <p style='color: #666; text-align: center;'>
                        Mã OTP có hiệu lực trong <strong>10 phút</strong>
                    </p>
                    <p style='color: #999; font-size: 12px; text-align: center;'>
                        Nếu bạn không yêu cầu đăng ký này, vui lòng bỏ qua email này.
                    </p>
                </div>
            </body>
            </html>";
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

        // ✅ Gửi OTP cho đăng ký (lưu thông tin đăng ký tạm thời)
        public async Task<OtpAttribute> SendRegistrationOtpAsync(string email, RegisterRequestDTO registerData)
        {
            try
            {
                // Kiểm tra email đã tồn tại chưa
                var existingUser = await _userRepository.GetUserByEmailAsync(email);
                if (existingUser != null)
                {
                    return new OtpAttribute
                    {
                        Success = false,
                        Message = "Email đã được sử dụng"
                    };
                }

                // Kiểm tra username đã tồn tại chưa
                var existingUsername = await _userRepository.GetUserAsync(registerData.UserName);
                if (existingUsername != null)
                {
                    return new OtpAttribute
                    {
                        Success = false,
                        Message = "Tên đăng nhập đã tồn tại"
                    };
                }

                var otp = GenerateOtp();
                var sent = await SendRegistrationEmailAsync(email, otp, registerData.UserName);

                if (!sent)
                {
                    return new OtpAttribute
                    {
                        Success = false,
                        Message = "Không thể gửi email. Vui lòng thử lại sau."
                    };
                }

                // Lưu OTP vào cache
                await StoreOtpAsync($"REGISTER_{email}", otp);

                // Lưu thông tin đăng ký tạm thời vào cache (10 phút)
                var registrationData = System.Text.Json.JsonSerializer.Serialize(registerData);
                var cacheKey = $"REGISTRATION_DATA_{email}";
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                };
                await _cache.SetStringAsync(cacheKey, registrationData, options);

                _logger.LogInformation("Đã gửi OTP đăng ký đến email: {Email}", email);

                return new OtpAttribute
                {
                    Success = true,
                    Message = "Mã OTP đã được gửi đến email của bạn. Vui lòng kiểm tra hộp thư."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi OTP đăng ký cho email: {Email}", email);
                return new OtpAttribute
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        // ✅ Xác thực OTP đăng ký và trả về thông tin đăng ký
        public async Task<OtpAttribute> VerifyRegistrationOtpAsync(string email, string otp)
        {
            try
            {
                var cacheKey = $"OTP_REGISTER_{email}";
                var cachedOtp = await _cache.GetStringAsync(cacheKey);

                if (string.IsNullOrEmpty(cachedOtp))
                {
                    return new OtpAttribute
                    {
                        Success = false,
                        Message = "OTP không tồn tại hoặc đã hết hạn"
                    };
                }

                if (cachedOtp != otp)
                {
                    return new OtpAttribute
                    {
                        Success = false,
                        Message = "OTP không chính xác"
                    };
                }

                // Lấy thông tin đăng ký từ cache
                var dataKey = $"REGISTRATION_DATA_{email}";
                var registrationDataJson = await _cache.GetStringAsync(dataKey);

                if (string.IsNullOrEmpty(registrationDataJson))
                {
                    return new OtpAttribute
                    {
                        Success = false,
                        Message = "Thông tin đăng ký đã hết hạn. Vui lòng đăng ký lại."
                    };
                }

                // Xóa OTP và data khỏi cache
                await _cache.RemoveAsync(cacheKey);
                await _cache.RemoveAsync(dataKey);

                _logger.LogInformation("Xác thực OTP đăng ký thành công cho email: {Email}", email);

                return new OtpAttribute
                {
                    Success = true,
                    Message = "Xác thực thành công",
                    Email = email,
                    Data = registrationDataJson // Trả về JSON data để service xử lý
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xác thực OTP đăng ký cho email: {Email}", email);
                return new OtpAttribute
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        // ✅ Gửi email đăng ký với OTP
        private async Task<bool> SendRegistrationEmailAsync(string email, string otp, string userName)
        {
            try
            {
                _logger.LogInformation("Bắt đầu gửi email đăng ký đến {Email}", email);

                using var mail = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = "Xác thực đăng ký tài khoản - Mã OTP",
                    Body = CreateRegistrationEmailBody(otp, userName),
                    IsBodyHtml = true
                };
                mail.To.Add(email);

                using var smtp = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.SenderPassword),
                    EnableSsl = _emailSettings.EnableSsl
                };

                await smtp.SendMailAsync(mail);

                _logger.LogInformation("Đã gửi email đăng ký thành công đến {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email đăng ký đến {Email}", email);
                return false;
            }
        }
    }
}
