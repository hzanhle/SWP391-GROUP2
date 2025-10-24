using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using BookingService.Models;
using BookingService.Models.ModelSettings;

namespace BookingService.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IOptions<EmailSettings> emailSettings,
            ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value ?? throw new ArgumentNullException(nameof(emailSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gửi email cơ bản
        /// </summary>
        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            return await SendEmailInternalAsync(toEmail, subject, body);
        }

        /// <summary>
        /// Gửi email hợp đồng với Google Drive link
        /// </summary>
        public async Task<bool> SendContractEmailAsync(
            string toEmail,
            string customerName,
            string contractNumber,
            string driveLink)
        {
            try
            {
                _logger.LogInformation(
                    "Chuẩn bị gửi email hợp đồng đến {Email} với Drive link",
                    toEmail);

                var subject = $"[Xác nhận] Hợp đồng điện tử {contractNumber}";
                var body = CreateContractEmailBody(customerName, contractNumber, driveLink);

                return await SendEmailInternalAsync(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email hợp đồng đến {Email}", toEmail);
                return false;
            }
        }

        #region Private Methods

        /// <summary>
        /// Core method gửi email (không attachment)
        /// </summary>
        private async Task<bool> SendEmailInternalAsync(
            string email,
            string subject,
            string body)
        {
            try
            {
                _logger.LogInformation(
                    "Bắt đầu gửi email (Subject: {Subject}) đến {Email}",
                    subject, email);

                if (string.IsNullOrWhiteSpace(email))
                {
                    _logger.LogWarning("Email recipient is empty");
                    return false;
                }

                using var mail = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mail.To.Add(email);

                using var smtp = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.SenderPassword),
                    EnableSsl = _emailSettings.EnableSsl
                };

                await smtp.SendMailAsync(mail);

                _logger.LogInformation("✅ Đã gửi email thành công đến {Email}", email);
                return true;
            }
            catch (SmtpException ex)
            {
                _logger.LogError(
                    ex,
                    "Lỗi SMTP khi gửi email đến {Email}. StatusCode: {StatusCode}",
                    email, ex.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi không xác định khi gửi email đến {Email}", email);
                return false;
            }
        }

        /// <summary>
        /// Tạo HTML email body với Google Drive link
        /// </summary>
        private string CreateContractEmailBody(
            string customerName,
            string contractNumber,
            string driveLink)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <style>
                    body {{ 
                        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
                        line-height: 1.6; 
                        color: #333; 
                        margin: 0;
                        padding: 0;
                    }}
                    .container {{ 
                        max-width: 600px; 
                        margin: 0 auto; 
                        background: #f9f9f9; 
                    }}
                    .header {{ 
                        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); 
                        color: white; 
                        padding: 30px; 
                        text-align: center; 
                    }}
                    .content {{ 
                        background: white; 
                        padding: 30px; 
                    }}
                    .footer {{ 
                        background: #f0f0f0; 
                        padding: 20px; 
                        text-align: center; 
                        font-size: 12px; 
                        color: #666; 
                    }}
                    .highlight {{ 
                        color: #667eea; 
                        font-weight: bold; 
                    }}
                    .button {{ 
                        display: inline-block; 
                        background: #667eea; 
                        color: white !important; 
                        padding: 14px 30px; 
                        text-decoration: none; 
                        border-radius: 6px; 
                        margin: 20px 0;
                        font-weight: bold;
                        font-size: 16px;
                    }}
                    .button:hover {{ 
                        background: #5568d3; 
                    }}
                    .info-box {{ 
                        background: #f8f9fa; 
                        border-left: 4px solid #667eea; 
                        padding: 15px; 
                        margin: 20px 0; 
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h2>✅ Hợp Đồng Được Xác Nhận</h2>
                    </div>
                    <div class='content'>
                        <p>Xin chào <span class='highlight'>{customerName}</span>,</p>
                        <p>Cảm ơn bạn đã hoàn tất thanh toán. Hợp đồng điện tử của bạn đã được tạo thành công!</p>
                        
                        <div class='info-box'>
                            <strong>📄 Thông tin hợp đồng:</strong><br>
                            Số hợp đồng: <span class='highlight'>{contractNumber}</span><br>
                            Ngày tạo: {DateTime.UtcNow.AddHours(7):dd/MM/yyyy HH:mm} (GMT+7)
                        </div>

                        <p style='text-align: center;'>
                            <a href='{driveLink}' class='button'>📥 Tải Hợp Đồng PDF</a>
                        </p>

                        <p style='color: #666; font-size: 14px; line-height: 1.8;'>
                            <strong>📌 Lưu ý:</strong><br>
                            • Vui lòng lưu lại hợp đồng để đối chiếu khi cần thiết<br>
                            • Link tải hợp đồng luôn có hiệu lực<br>
                            • Nếu có thắc mắc, vui lòng liên hệ bộ phận hỗ trợ khách hàng
                        </p>

                        <p style='color: #999; font-size: 13px; margin-top: 30px; text-align: center;'>
                            🚗 Chúc bạn có chuyến đi an toàn và vui vẻ! 🎉
                        </p>
                    </div>
                    <div class='footer'>
                        <p>© 2025 EV Rental System. All rights reserved.</p>
                        <p style='color: #999; margin-top: 10px;'>
                            Đây là email tự động, vui lòng không trả lời email này.
                        </p>
                    </div>
                </div>
            </body>
            </html>";
        }

        #endregion
    }
}