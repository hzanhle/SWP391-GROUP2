using System.ComponentModel.DataAnnotations;
namespace StationService.DTOs
{
    public class UpdateStationRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Tên trạm phải từ 5 đến 200 ký tự.")]
        public int Id { get; set; }
        public string Name { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Vị trí trạm phải từ 10 đến 500 ký tự.")]
        public string Location { get; set; }
        public int? ManagerId { get; set; } // User với role là Staff
        public bool IsActive { get; set; } // Dùng để kích hoạt/vô hiệu hóa trạng thái hoạt động của trạm

    }
}
