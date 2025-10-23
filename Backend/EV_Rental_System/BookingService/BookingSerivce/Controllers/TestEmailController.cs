using Microsoft.AspNetCore.Mvc;
using BookingService.Services;

namespace BookingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestEmailController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<TestEmailController> _logger;

        public TestEmailController(
            IEmailService emailService,
            ILogger<TestEmailController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Test gửi email đơn giản
        /// GET: api/testemail/send?email=test@example.com
        /// </summary>
        [HttpGet("send")]
        public async Task<IActionResult> SendTestEmail([FromQuery] string email = "qvuong23102004@gmail.com")
        {
            try
            {
                _logger.LogInformation("🧪 Testing email send to {Email}", email);

                var subject = "🎉 Test Email from BookingService";
                var body = @"
                    <html>
                    <body style='font-family: Arial; padding: 20px;'>
                        <h2 style='color: #4CAF50;'>✅ Email Test Successful!</h2>
                        <p>This is a test email from <strong>BookingService</strong>.</p>
                        <p>If you receive this email, the email service is working correctly!</p>
                        <hr>
                        <p style='color: #666; font-size: 12px;'>
                            Sent at: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"
                        </p>
                    </body>
                    </html>";

                var result = await _emailService.SendEmailAsync(email, subject, body);

                if (result)
                {
                    _logger.LogInformation("✅ Test email sent successfully to {Email}", email);
                    return Ok(new
                    {
                        success = true,
                        message = $"Email sent successfully to {email}",
                        timestamp = DateTime.Now
                    });
                }
                else
                {
                    _logger.LogWarning("❌ Failed to send test email to {Email}", email);
                    return BadRequest(new
                    {
                        success = false,
                        message = "Failed to send email. Check logs for details.",
                        timestamp = DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error sending test email to {Email}", email);
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error: {ex.Message}",
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Test gửi email với attachment (file PDF giả)
        /// POST: api/testemail/send-with-attachment
        /// </summary>
        [HttpPost("send-with-attachment")]
        public async Task<IActionResult> SendTestEmailWithAttachment(
            [FromQuery] string email = "qvuong23102004@gmail.com")
        {
            try
            {
                _logger.LogInformation("🧪 Testing email with attachment to {Email}", email);

                // Tạo file PDF giả để test
                var tempFile = Path.Combine(Path.GetTempPath(), $"test_{DateTime.Now:yyyyMMddHHmmss}.pdf");
                await System.IO.File.WriteAllTextAsync(tempFile, "This is a test PDF file");

                var subject = "🎉 Test Email with Attachment";
                var body = @"
                    <html>
                    <body style='font-family: Arial; padding: 20px;'>
                        <h2 style='color: #4CAF50;'>✅ Email Test with Attachment!</h2>
                        <p>This email contains a test PDF attachment.</p>
                        <p>If you receive this with an attachment, the email service is working correctly!</p>
                    </body>
                    </html>";

                // Gửi qua ContractEmailAsync để test attachment
                var result = await _emailService.SendContractEmailAsync(
                    toEmail: email,
                    customerName: "Test User",
                    contractNumber: "TEST-001",
                    absoluteFilePath: tempFile);

                // Xóa file tạm
                if (System.IO.File.Exists(tempFile))
                    System.IO.File.Delete(tempFile);

                if (result)
                {
                    _logger.LogInformation("✅ Test email with attachment sent successfully to {Email}", email);
                    return Ok(new
                    {
                        success = true,
                        message = $"Email with attachment sent successfully to {email}",
                        timestamp = DateTime.Now
                    });
                }
                else
                {
                    _logger.LogWarning("❌ Failed to send test email with attachment to {Email}", email);
                    return BadRequest(new
                    {
                        success = false,
                        message = "Failed to send email. Check logs for details.",
                        timestamp = DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error sending test email with attachment to {Email}", email);
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error: {ex.Message}",
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Test gửi email SPAM (nhiều email liên tục)
        /// POST: api/testemail/spam?count=5
        /// </summary>
        [HttpPost("spam")]
        public async Task<IActionResult> SendSpamEmails(
            [FromQuery] string email = "qvuong23102004@gmail.com",
            [FromQuery] int count = 3)
        {
            try
            {
                if (count > 10)
                {
                    return BadRequest("Maximum 10 emails allowed for testing");
                }

                _logger.LogInformation("🧪 Sending {Count} spam emails to {Email}", count, email);

                var results = new List<object>();

                for (int i = 1; i <= count; i++)
                {
                    var subject = $"🎉 Spam Test Email #{i}/{count}";
                    var body = $@"
                        <html>
                        <body style='font-family: Arial; padding: 20px;'>
                            <h2 style='color: #FF5722;'>📧 Spam Email #{i}/{count}</h2>
                            <p>This is spam test email number <strong>{i}</strong> of <strong>{count}</strong>.</p>
                            <p>Sent at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
                        </body>
                        </html>";

                    var result = await _emailService.SendEmailAsync(email, subject, body);

                    results.Add(new
                    {
                        emailNumber = i,
                        success = result,
                        timestamp = DateTime.Now
                    });

                    // Delay 1 giây giữa các email để tránh bị Gmail chặn
                    if (i < count)
                        await Task.Delay(1000);
                }

                var successCount = results.Count(r => ((dynamic)r).success);

                return Ok(new
                {
                    success = true,
                    message = $"Sent {successCount}/{count} emails successfully",
                    results = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error sending spam emails");
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Kiểm tra cấu hình email (không gửi email)
        /// GET: api/testemail/check-config
        /// </summary>
        [HttpGet("check-config")]
        public IActionResult CheckEmailConfig()
        {
            try
            {
                // Đọc cấu hình từ IConfiguration
                var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                var emailSettings = config.GetSection("EmailSettings");

                return Ok(new
                {
                    success = true,
                    configuration = new
                    {
                        smtpServer = emailSettings["SmtpServer"] ?? "NOT SET",
                        smtpPort = emailSettings["SmtpPort"] ?? "NOT SET",
                        senderEmail = emailSettings["SenderEmail"] ?? "NOT SET",
                        senderName = emailSettings["SenderName"] ?? "NOT SET",
                        enableSsl = emailSettings["EnableSsl"] ?? "NOT SET",
                        passwordLength = emailSettings["SenderPassword"]?.Length ?? 0
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }
    }
}

/*
HƯỚNG DẪN SỬ DỤNG:

1. Test gửi email đơn giản:
   GET http://localhost:5049/api/testemail/send?email=test@gmail.com

2. Test gửi email với attachment:
   POST http://localhost:5049/api/testemail/send-with-attachment?email=test@gmail.com

3. Test gửi nhiều email (spam):
   POST http://localhost:5049/api/testemail/spam?email=test@gmail.com&count=3

4. Kiểm tra cấu hình email:
   GET http://localhost:5049/api/testemail/check-config

5. Xem log trong console để debug
*/