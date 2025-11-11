using System.ComponentModel.DataAnnotations;

namespace AdminDashboardService.ExternalModels.BookingServiceModels
{
    /// <summary>
    /// External model for TrustScore from BookingService (read-only)
    /// </summary>
    public class TrustScore
    {
        [Key]
        public int TrustScoreId { get; set; }

        public int UserId { get; set; }
        public int Score { get; set; }
        public int OrderId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
