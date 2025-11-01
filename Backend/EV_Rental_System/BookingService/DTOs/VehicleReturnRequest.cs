namespace BookingService.DTOs
{
    /// <summary>
    /// Request cho việc trả xe (return)
    /// </summary>
    public class VehicleReturnRequest
    {
        /// <summary>
        /// Số km khi trả xe (optional)
        /// </summary>
        public int? OdometerReading { get; set; }

        /// <summary>
        /// Mức nhiên liệu/pin khi trả xe (%) (optional)
        /// </summary>
        public int? FuelLevel { get; set; }

        /// <summary>
        /// Mô tả tình trạng xe khi trả (optional)
        /// </summary>
        public string? ConditionNotes { get; set; }

        /// <summary>
        /// Có hư hỏng không?
        /// </summary>
        public bool HasDamage { get; set; }

        /// <summary>
        /// Mô tả chi tiết hư hỏng (nếu có)
        /// </summary>
        public string? DamageDescription { get; set; }

        /// <summary>
        /// Phí bồi thường hư hỏng (nếu có)
        /// </summary>
        public decimal DamageCharge { get; set; }

        /// <summary>
        /// Danh sách ảnh xe khi trả (required)
        /// Frontend sẽ gửi lên qua form-data với key là "images"
        /// </summary>
        // Note: IFormFile list will be handled separately in controller [FromForm]
    }
}
