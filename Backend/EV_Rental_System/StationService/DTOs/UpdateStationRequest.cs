using System.ComponentModel.DataAnnotations;
namespace StationService.DTOs
{
    public class UpdateStationRequest
    {
        public int Id { get; set; }
        [Required]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Tên trạm phải từ 5 đến 200 ký tự.")]
        public required string Name { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Vị trí trạm phải từ 10 đến 500 ký tự.")]
        public required string Location { get; set; }
        public int? ManagerId { get; set; } // User với role là Employee
        public bool IsActive { get; set; } // Dùng để kích hoạt/vô hiệu hóa trạng thái hoạt động của trạm

        [Range(-90, 90, ErrorMessage = "Lat phải trong [-90, 90].")]
        public double? Lat { get; set; }

        [Range(-180, 180, ErrorMessage = "Lng phải trong [-180, 180].")]
        public double? Lng { get; set; }

    }
}
