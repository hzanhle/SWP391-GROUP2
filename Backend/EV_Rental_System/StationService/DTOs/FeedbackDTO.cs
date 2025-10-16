namespace StationService.DTOs
{
    public class FeedbackDTO
    {
        public int FeedbackId { get; set; }
        public int StationId { get; set; }
        public int OrderId { get; set; }
        public int Rate { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
