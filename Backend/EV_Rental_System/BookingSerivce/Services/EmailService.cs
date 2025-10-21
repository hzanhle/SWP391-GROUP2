using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using BookingService.Models;
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
        /// Gửi email đơn giản (dùng chung)
        /// </summary>
        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            return await SendEmailInternalAsync(toEmail, subject, body, attachmentPath: null);
        }

        /// <summary>
        /// Gửi email hợp đồng với file PDF đính kèm
        /// </summary>
        public async Task<bool> SendContractEmailAsync(
            string toEmail,
            string customerName,
            string contractNumber,
            string absoluteFilePath)
        {
            var subject = $"[Xác nhận] Hợp đồng điện tử {contractNumber}";
            var body = CreateContractEmailBody(customerName, contractNumber);

            return await SendEmailInternalAsync(toEmail, subject, body, absoluteFilePath);
        }

        #region Private Methods

        /// <summary>
        /// Logic gửi email chung (hỗ trợ đính kèm file)
        /// </summary>
        private async Task<bool> SendEmailInternalAsync(
            string email,
            string subject,
            string body,
            string? attachmentPath = null)
        {
            try
            {
                _logger.LogInformation(
                    "Bắt đầu gửi email (Subject: {Subject}) đến {Email}",
                    subject, email);

                // Validate email
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

                // Thêm file đính kèm nếu có
                if (!string.IsNullOrEmpty(attachmentPath))
                {
                    if (File.Exists(attachmentPath))
                    {
                        var attachment = new Attachment(attachmentPath);
                        mail.Attachments.Add(attachment);
                        _logger.LogInformation("Đã đính kèm file: {FilePath}", attachmentPath);
                    }
                    else
                    {
                        _logger.LogWarning("File không tồn tại: {FilePath}", attachmentPath);
                    }
                }

                // Gửi email qua SMTP
                using var smtp = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    Credentials = new NetworkCredential(
                        _emailSettings.SenderEmail,
                        _emailSettings.SenderPassword),
                    EnableSsl = _emailSettings.EnableSsl
                };

                await smtp.SendMailAsync(mail);

                _logger.LogInformation("Đã gửi email thành công đến {Email}", email);
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
        /// Tạo template HTML cho email hợp đồng
        /// </summary>
        private string CreateContractEmailBody(string customerName, string contractNumber)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <style>
                    body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; background: #f9f9f9; padding: 0; }}
                    .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; }}
                    .content {{ background: white; padding: 30px; }}
                    .footer {{ background: #f0f0f0; padding: 20px; text-align: center; font-size: 12px; color: #666; }}
                    .highlight {{ color: #667eea; font-weight: bold; }}
                    .button {{ display: inline-block; background: #667eea; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h2>✓ Hợp Đồng Được Xác Nhận</h2>
                    </div>
                    <div class='content'>
                        <p>Xin chào <span class='highlight'>{customerName}</span>,</p>
                        <p>Cảm ơn bạn đã hoàn tất thanh toán. Hợp đồng điện tử của bạn đã được tạo thành công.</p>
                        <p>
                            <strong>Thông tin hợp đồng:</strong><br>
                            Số hợp đồng: <span class='highlight'>{contractNumber}</span>
                        </p>
                        <p>
                            File PDF đính kèm trong email này. Vui lòng lưu lại để đối chiếu khi cần thiết.
                        </p>
                        <p>
                            <strong>Lưu ý:</strong> Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ bộ phận hỗ trợ khách hàng của chúng tôi.
                        </p>
                        <p style='color: #999; font-size: 13px;'>
                            Chúc bạn có một chuyến đi an toàn và vui vẻ!
                        </p>
                    </div>
                    <div class='footer'>
                        <p>© 2025 Booking System. All rights reserved.</p>
                        <p>Đây là email tự động, vui lòng không trả lời email này.</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        #endregion
    }
}