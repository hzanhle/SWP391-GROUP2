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
        /// Đang xử lý hoàn tiền tự động qua VNPay API
        /// </summary>
        Processing = 1,

        /// <summary>
        /// Đã hoàn tiền thành công
        /// </summary>
        Processed = 2,

        /// <summary>
        /// Hoàn tiền thất bại
        /// </summary>
        Failed = 3,

        /// <summary>
        /// Chờ admin upload minh chứng hoàn tiền thủ công
        /// </summary>
        AwaitingManualProof = 4,

        /// <summary>
        /// Không cần hoàn tiền (deposit = 0 hoặc customer owes money)
        /// </summary>
        NotRequired = 5
    }
}
