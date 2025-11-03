namespace BookingService.Models
{
    /// <summary>
    /// Phương thức hoàn tiền deposit
    /// </summary>
    public enum RefundMethod
    {
        /// <summary>
        /// Chưa xác định phương thức
        /// </summary>
        NotSet = 0,

        /// <summary>
        /// Hoàn tiền tự động qua VNPay Refund API
        /// </summary>
        Automatic = 1,

        /// <summary>
        /// Hoàn tiền thủ công qua VNPay portal (cần minh chứng)
        /// </summary>
        Manual = 2
    }
}
