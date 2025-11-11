namespace BookingService.DTOs
{
    /// <summary>
    /// Response containing a user's current trust score
    /// </summary>
    public class TrustScoreResponse
    {
        public int UserId { get; set; }
        public int Score { get; set; }
        public int LastRelatedOrderId { get; set; }
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Reputation level based on score
        /// </summary>
        public string ReputationLevel => Score switch
        {
            >= 200 => "Excellent",
            >= 150 => "Good",
            >= 100 => "Average",
            >= 50 => "Below Average",
            _ => "Poor"
        };

        /// <summary>
        /// Recent score changes (last 5)
        /// </summary>
        public List<TrustScoreHistoryResponse>? RecentHistory { get; set; }
    }
}
