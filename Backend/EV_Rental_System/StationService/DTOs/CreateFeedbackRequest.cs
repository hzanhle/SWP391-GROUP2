using System.ComponentModel.DataAnnotations;

namespace StationService.DTOs
{
    public class CreateFeedbackRequest
    {
        [Required]
        public int OrderId { get; set; }
        [Required]
        [Range(1, 5)]
        public int Rate { get; set; }
        [StringLength(1000)]
        public string? Description { get; set; }
    }
}
