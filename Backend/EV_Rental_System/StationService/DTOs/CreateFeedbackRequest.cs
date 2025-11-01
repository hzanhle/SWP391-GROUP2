using System.ComponentModel.DataAnnotations;

namespace StationService.DTOs
{
    public class CreateFeedbackDTO
    {
        [Required(ErrorMessage = "StationId là bắt buộc")]
        public int StationId { get; set; }

        [Required(ErrorMessage = "Rate là bắt buộc")]
        [Range(1, 5, ErrorMessage = "Rating phải từ 1-5 sao")]
        public int Rate { get; set; }

        [MaxLength(1000, ErrorMessage = "Mô tả không quá 1000 ký tự")]
        public string? Description { get; set; }
    }
}
