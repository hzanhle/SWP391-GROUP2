namespace BookingService.DTOs
{
    public class ConfirmPaymentRequest
    {
        public int OrderId { get; set; }               // ID của đơn hàng
        public string TransactionId { get; set; }      // Mã giao dịch từ cổng thanh toán (VD: VNPay, Momo)
        public string? GatewayResponse { get; set; }   // Dữ liệu phản hồi JSON / mã xác nhận từ cổng thanh toán
    }
}