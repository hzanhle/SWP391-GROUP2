namespace BookingService.Services
{
    public interface IEmailService
    {
        /// <summary>
        /// Gửi một email đơn giản (giống OTP).
        /// </summary>
        Task<bool> SendEmailAsync(string toEmail, string subject, string body);

        /// <summary>
        /// Gửi email có đính kèm file (dùng cho hợp đồng).
        /// </summary>
        Task<bool> SendContractEmailAsync(
            string toEmail,
            string customerName,
            string contractNumber,
            string absoluteFilePath // Đường dẫn tuyệt đối đến file PDF
        );
    }
}