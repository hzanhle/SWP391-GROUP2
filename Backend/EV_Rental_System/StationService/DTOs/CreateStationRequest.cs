using System.ComponentModel.DataAnnotations;
namespace StationService.DTOs
{
    public class CreateStationRequest
    {
        [Required(ErrorMessage = "Tên trạm không được để trống.")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Tên trạm phải từ 5 đến 200 ký tự.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vị trí trạm không được để trống.")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Vị trí trạm phải từ 10 đến 500 ký tự.")]
        public string Location { get; set; }
        public int? ManagerId { get; set; } // User với role là Employee
    }
}