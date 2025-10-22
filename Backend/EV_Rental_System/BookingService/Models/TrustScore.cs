namespace BookingService.Models
{
    public class TrustScore
    {
        public int TrustScoreId { get; set; }
        public int UserId { get; set; }
        public int Score { get; set; } = 0; // Default score is 0
        public int OrderId { get; set; } // Last related order
        public DateTime CreatedAt { get; set; } // When the score was created or last updated

        public TrustScore()
        {          
        }

        public TrustScore(int score, int orderId)
        {
            Score = score;
            OrderId = orderId;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
