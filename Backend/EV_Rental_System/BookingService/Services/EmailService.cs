using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using BookingService.Models.ModelSettings;

namespace BookingService.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _logger.LogWarning(
                "📧 Email Config Loaded => Email: {Email}, Pass: {HasPass}, From: {SenderName}",
                string.IsNullOrWhiteSpace(_settings.SenderEmail) ? "(empty)" : _settings.SenderEmail,
                string.IsNullOrWhiteSpace(_settings.SenderPassword) ? "❌ No Password" : "✅ Has Password",
                _settings.SenderName
            );
        }

        // ------------------ Public Methods ------------------

        public Task<bool> SendEmailAsync(string toEmail, string subject, string body)
            => SendEmailInternalAsync(toEmail, subject, body);

        public async Task<bool> SendContractEmailAsync(string toEmail, string customerName, string contractNumber, string driveLink)
        {
            try
            {
                var subject = $"[Xác nhận] Hợp đồng điện tử {contractNumber}";
                var body = CreateContractEmailBody(customerName, contractNumber, driveLink);

                return await SendEmailInternalAsync(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Lỗi khi gửi email hợp đồng đến {Email}", toEmail);
                return false;
            }
        }

        // ------------------ Core Send Logic ------------------

        private async Task<bool> SendEmailInternalAsync(string toEmail, string subject, string body)
        {
            // Validate cấu hình trước khi tạo MailAddress
            if (string.IsNullOrWhiteSpace(_settings.SenderEmail))
            {
                _logger.LogError("❌ SenderEmail is empty. Configure EmailSettings:SenderEmail.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(_settings.SenderPassword))
            {
                _logger.LogError("❌ SenderPassword is empty. Configure EmailSettings:SenderPassword (use user-secrets).");
                return false;
            }
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                _logger.LogWarning("⚠️ Email người nhận trống, bỏ qua gửi.");
                return false;
            }

            try
            {
                using var mail = new MailMessage
                {
                    From = new MailAddress(_settings.SenderEmail.Trim(), _settings.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mail.To.Add(toEmail);

                // Chọn chế độ SMTP theo cổng:
                // - 587: STARTTLS (EnableSsl = true) => Gmail yêu cầu MustIssueSTARTTLSFirst
                // - 465: SSL implicit (EnableSsl = true)
                var host = _settings.SmtpServer;
                var port = _settings.SmtpPort > 0 ? _settings.SmtpPort : 587;
                var enableSsl = _settings.EnableSsl; // nên là true

                using var smtp = new SmtpClient(host, port)
                {
                    EnableSsl = enableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_settings.SenderEmail.Trim(), _settings.SenderPassword),
                    Timeout = 15000
                };

                // Hint cho một số môi trường để chắc chắn SMTP chọn STARTTLS trước AUTH khi dùng 587
                if (host.Equals("smtp.gmail.com", StringComparison.OrdinalIgnoreCase) && port == 587 && enableSsl)
                {
                    smtp.TargetName = "STARTTLS/smtp.gmail.com";
                }

                _logger.LogInformation(
                    "📨 Đang gửi email đến {Email}... (smtp: {Host}:{Port}, ssl: {Ssl})",
                    toEmail, host, port, enableSsl
                );

                await smtp.SendMailAsync(mail);

                _logger.LogInformation("✅ Gửi email thành công đến {Email}", toEmail);
                return true;
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "❌ Lỗi SMTP ({StatusCode}) khi gửi đến {Email}", ex.StatusCode, toEmail);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Lỗi không xác định khi gửi email đến {Email}", toEmail);
                return false;
            }
        }

        // ------------------ Email Template ------------------

        private string CreateContractEmailBody(string customerName, string contractNumber, string driveLink)
        {
            var currentTime = DateTime.UtcNow.AddHours(7).ToString("dd/MM/yyyy HH:mm");

            return $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
<meta charset='UTF-8'>
<style>
    body {{
        font-family: 'Segoe UI', Tahoma, sans-serif;
        color: #333;
        background-color: #f9f9f9;
        margin: 0;
        padding: 0;
    }}
    .container {{
        max-width: 600px;
        background: #fff;
        margin: 20px auto;
        border-radius: 10px;
        overflow: hidden;
        box-shadow: 0 2px 10px rgba(0,0,0,0.1);
    }}
    .header {{
        background: linear-gradient(135deg, #667eea, #764ba2);
        color: white;
        padding: 30px;
        text-align: center;
    }}
    .content {{
        padding: 30px;
        line-height: 1.6;
    }}
    .button {{
        display: inline-block;
        background-color: #667eea;
        color: white;
        padding: 14px 28px;
        border-radius: 6px;
        text-decoration: none;
        font-weight: bold;
        margin-top: 20px;
    }}
    .info-box {{
        background: #f2f4ff;
        padding: 15px;
        border-left: 4px solid #667eea;
        margin: 20px 0;
    }}
    .footer {{
        background-color: #f0f0f0;
        padding: 15px;
        text-align: center;
        font-size: 13px;
        color: #777;
    }}
</style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Hợp Đồng Được Xác Nhận ✅</h2>
        </div>
        <div class='content'>
            <p>Xin chào <strong>{customerName}</strong>,</p>
            <p>Hợp đồng điện tử của bạn đã được tạo thành công.</p>
            <div class='info-box'>
                <b>Mã hợp đồng:</b> {contractNumber}<br>
                <b>Thời gian:</b> {currentTime} (GMT+7)
            </div>
            <p style='text-align:center'>
                <a href='{driveLink}' class='button'>📥 Tải Hợp Đồng PDF</a>
            </p>
            <p style='font-size:14px;color:#666'>
                • Vui lòng lưu lại hợp đồng để đối chiếu khi cần thiết<br>
                • Link tải hợp đồng luôn có hiệu lực<br>
                • Nếu có thắc mắc, vui lòng liên hệ bộ phận hỗ trợ khách hàng
            </p>
        </div>
        <div class='footer'>
            © 2025 EV Rental System — Email tự động, vui lòng không trả lời.
        </div>
    </div>
</body>
</html>";
        }
    }
}
