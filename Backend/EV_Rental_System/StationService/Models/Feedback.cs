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
        public int OrderId { get; set; }

        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao.")]
        public int Rate { get; set; } // đánh giá (1-5 sao)
        public string? Description { get; set; } // mô tả bình luận
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // ---Navigation properties--- thuộc tính điều hướng
        public Station? Station { get; set; } = null;
        public Feedback()
        {
            CreatedDate = DateTime.UtcNow;
        }
    }
}
