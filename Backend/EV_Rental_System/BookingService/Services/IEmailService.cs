namespace BookingService.Services
{
    public interface IEmailService
    {
        /// <summary>
        /// Gửi email cơ bản
        /// </summary>
        Task<bool> SendEmailAsync(string toEmail, string subject, string body);

        /// <summary>
        /// Gửi email hợp đồng với Google Drive link
        /// </summary>
        Task<bool> SendContractEmailAsync(
            string toEmail,
            string customerName,
            string contractNumber,
            string driveLink);
    }
}