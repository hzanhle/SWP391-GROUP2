using System.ComponentModel.DataAnnotations;

namespace StationService.DTOs
{
    public class UpdateFeedbackRequest
    {
        [Required(ErrorMessage = "Đánh giá không được để trống.")]
        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao.")]
        public int Rate { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự.")]
        public string? Description { get; set; }
    }
}
