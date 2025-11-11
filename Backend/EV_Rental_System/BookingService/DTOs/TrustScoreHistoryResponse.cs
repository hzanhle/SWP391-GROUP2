namespace BookingService.DTOs
{
    /// <summary>
    /// Response for trust score change history entry
    /// </summary>
    public class TrustScoreHistoryResponse
    {
        public int HistoryId { get; set; }
        public int UserId { get; set; }
        public int? OrderId { get; set; }
        public int ChangeAmount { get; set; }
        public int PreviousScore { get; set; }
        public int NewScore { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string ChangeType { get; set; } = string.Empty;
        public int? AdjustedByAdminId { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Formatted change for display (e.g., "+10", "-5")
        /// </summary>
        public string FormattedChange => ChangeAmount >= 0 ? $"+{ChangeAmount}" : $"{ChangeAmount}";
    }
}
