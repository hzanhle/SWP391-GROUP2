using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace UserService.Models
{
    using System.ComponentModel.DataAnnotations;

    namespace UserService.Models
    {
        public class CitizenInfo
        {
            [Key]
            public int Id { get; set; }

            [Required(ErrorMessage = "UserId là bắt buộc")]
            public int UserId { get; set; }

            public User? User { get; set; }

            [Required(ErrorMessage = "Số CCCD là bắt buộc")]
            [StringLength(12, MinimumLength = 9, ErrorMessage = "Số CCCD phải từ 9-12 ký tự")]
            [RegularExpression(@"^[0-9]{9,12}$", ErrorMessage = "Số CCCD chỉ được chứa số")]
            public string CitizenId { get; set; }

            [Required(ErrorMessage = "Giới tính là bắt buộc")]
            [RegularExpression(@"^(Nam|Nữ|Khác)$", ErrorMessage = "Giới tính phải là Nam, Nữ hoặc Khác")]
            public string Sex { get; set; }

            [Required(ErrorMessage = "Ngày đăng ký CCCD là bắt buộc")]
            public DateOnly CitiRegisDate { get; set; }

            [Required(ErrorMessage = "Nơi đăng ký CCCD là bắt buộc")]
            [StringLength(200, ErrorMessage = "Nơi đăng ký không được vượt quá 200 ký tự")]
            public string CitiRegisOffice { get; set; }

            [Required(ErrorMessage = "Họ tên là bắt buộc")]
            [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2-100 ký tự")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
            [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự")]
            public string Address { get; set; }

            [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
            public DateOnly DayOfBirth { get; set; }

            [Required(ErrorMessage = "Trạng thái là bắt buộc")]
            [StringLength(50, ErrorMessage = "Trạng thái không được vượt quá 50 ký tự")]
            public string Status { get; set; }

            public bool? IsApproved { get; set; }

            public DateTime DayCreated { get; set; }

            public ICollection<Image> Images { get; set; } = new List<Image>();

            public CitizenInfo() { }

            public CitizenInfo(int userId, string sex, string status, string citizenId,
                               DateOnly citiRegisDate, string citiRegisOffice, string fullName,
                               string address, DateOnly dayOfBirth, bool isApproved, DateTime dayCreated)
            {
                UserId = userId;
                Sex = sex;
                CitizenId = citizenId;
                CitiRegisDate = citiRegisDate;
                CitiRegisOffice = citiRegisOffice;
                FullName = fullName;
                Status = status;
                Address = address;
                DayOfBirth = dayOfBirth;
                IsApproved = isApproved;
                DayCreated = dayCreated;
            }
        }
    }

}
