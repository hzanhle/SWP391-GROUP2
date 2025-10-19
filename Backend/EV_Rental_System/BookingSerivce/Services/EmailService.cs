using BookingService.Models; // Cần cho EmailSettings
using BookingService.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace UserService.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IOptions<EmailSettings> emailSettings,
            ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Gửi email đơn giản (dùng chung).
        /// </summary>
        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            return await SendEmailInternalAsync(toEmail, subject, body, null);
        }

        /// <summary>
        /// Phương thức chuyên biệt để gửi email hợp đồng.
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

        // ========== PRIVATE METHODS (Lấy từ OtpService của bạn) ==========

        /// <summary>
        /// (REFACTOR) Logic gửi email GỐC, được dùng chung.
        /// (MỚI) Thêm tham số `attachmentPath`.
        /// </summary>
        private async Task<bool> SendEmailInternalAsync(string email, string subject, string body, string? attachmentPath = null)
        {
            try
            {
                _logger.LogInformation("Bắt đầu gửi email (Subject: {Subject}) đến {Email}", subject, email);

                using var mail = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mail.To.Add(email);

                // --- (LOGIC MỚI) THÊM ATTACHMENT ---
                if (!string.IsNullOrEmpty(attachmentPath) && File.Exists(attachmentPath))
                {
                    var attachment = new Attachment(attachmentPath);
                    mail.Attachments.Add(attachment);
                    _logger.LogInformation("Đã đính kèm file: {File}", attachmentPath);
                }
                // ------------------------------------

                using var smtp = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.SenderPassword),
                    EnableSsl = _emailSettings.EnableSsl
                };

                await smtp.SendMailAsync(mail);

                _logger.LogInformation("Đã gửi email thành công đến {Email}", email);
                return true;
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "Lỗi SMTP khi gửi email đến {Email}. StatusCode: {StatusCode}",
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
        /// (MỚI) Template email cho hợp đồng.
        /// </summary>
        private string CreateContractEmailBody(string customerName, string contractNumber)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <body style='font-family: Arial; padding: 20px;'>
                <div style='max-width: 600px; margin: 0 auto; background: white; padding: 30px; border-radius: 10px;'>
                    <h2 style='color: #333; text-align: center;'>Hoàn tất Hợp đồng Thuê xe</h2>
                    <p style='color: #666; text-align: center;'>
                        Xin chào <strong>{customerName}</strong>,
                    </p>
                    <p style='color: #666; text-align: center;'>
                        Cảm ơn bạn đã hoàn tất thanh toán. Hợp đồng điện tử của bạn (số: <strong>{contractNumber}</strong>)
                        đã được tạo thành công và đính kèm trong email này.
                    </p>
                    <p style='color: #666; text-align: center;'>
                        Vui lòng lưu lại file PDF để đối chiếu khi cần thiết.
                    </p>
                    <p style='color: #999; font-size: 12px; text-align: center;'>
                        Chúc bạn có một chuyến đi an toàn và vui vẻ!
                    </p>
                </div>
            </body>
            </html>";
        }
    }
}