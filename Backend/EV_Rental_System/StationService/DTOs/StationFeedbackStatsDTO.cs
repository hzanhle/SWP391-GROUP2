namespace StationService.DTOs
{
    public class StationFeedbackStatsDTO
    {
        public int StationId { get; set; }
        public string? StationName { get; set; }
        public int TotalFeedbacks { get; set; }
        public double AverageRating { get; set; }

        // Phân bố rating
        public int FiveStar { get; set; }
        public int FourStar { get; set; }
        public int ThreeStar { get; set; }
        public int TwoStar { get; set; }
        public int OneStar { get; set; }
    }
}
