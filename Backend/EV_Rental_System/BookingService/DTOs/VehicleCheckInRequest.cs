namespace BookingService.DTOs
{
    /// <summary>
    /// Request cho việc nhận xe (check-in)
    /// </summary>
    public class VehicleCheckInRequest
    {
        /// <summary>
        /// Số km hiện tại (optional)
        /// </summary>
        public int? OdometerReading { get; set; }

        /// <summary>
        /// Mức nhiên liệu/pin hiện tại (%) (optional)
        /// </summary>
        public int? FuelLevel { get; set; }

        /// <summary>
        /// Ghi chú về tình trạng xe (optional)
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Danh sách ảnh xe (required)
        /// Frontend sẽ gửi lên qua form-data với key là "images"
        /// </summary>
        // Note: IFormFile list will be handled separately in controller [FromForm]
    }
}
