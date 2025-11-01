// Models/Feedback.cs
using System.ComponentModel.DataAnnotations;

namespace StationService.Models
{
    public class Feedback
    {
        [Key]
        public int FeedbackId { get; set; }

        [Required]
        public int StationId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao.")]
        public int Rate { get; set; }

        [MaxLength(1000, ErrorMessage = "Mô tả không quá 1000 ký tự.")]
        public string? Description { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public bool IsVerified { get; set; } = false; // Admin có thể verify feedback
        public bool IsPublished { get; set; } = true;  // Có thể ẩn feedback spam

        public Station? Station { get; set; }

        public Feedback()
        {
            CreatedDate = DateTime.UtcNow;
        }
    }
}