namespace BookingService.Models
{
    /// <summary>
    /// Trạng thái hoàn tiền deposit
    /// </summary>
    public enum RefundStatus
    {
        /// <summary>
        /// Chờ xử lý hoàn tiền
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Đã hoàn tiền thành công
        /// </summary>
        Processed = 1,

        /// <summary>
        /// Hoàn tiền thất bại
        /// </summary>
        Failed = 2,

        /// <summary>
        /// Không cần hoàn tiền (deposit = 0 hoặc customer owes money)
        /// </summary>
        NotRequired = 3
    }
}
