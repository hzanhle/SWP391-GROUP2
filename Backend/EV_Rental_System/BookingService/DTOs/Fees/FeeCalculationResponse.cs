namespace BookingService.DTOs.Fees
{
    /// <summary>
    /// Response containing calculated fees
    /// </summary>
    public class FeeCalculationResponse
    {
        public int OrderId { get; set; }
        public List<FeeDto> CalculatedFees { get; set; } = new();
        public decimal TotalFees { get; set; }
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Details about each fee calculation
        /// </summary>
        public Dictionary<string, string> CalculationDetails { get; set; } = new();
    }
}
