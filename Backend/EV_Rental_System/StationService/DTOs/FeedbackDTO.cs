// DTOs/FeedbackDTO.cs

using System.ComponentModel.DataAnnotations;

namespace StationService.DTOs
{
    /// <summary>
    /// Feedback response DTO
    /// </summary>
    public class FeedbackDTO
    {
        public int FeedbackId { get; set; }
        public int StationId { get; set; }
        public string? StationName { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public int Rate { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsVerified { get; set; }      // Admin đã verify chưa
        public bool IsPublished { get; set; }     // Có hiển thị không
    }
}