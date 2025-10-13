using System.ComponentModel.DataAnnotations;

namespace UserService.Models
{
    public class DriverLicense
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "UserId là bắt buộc")]
        public int UserId { get; set; }

        public User? User { get; set; }

        [Required(ErrorMessage = "Số giấy phép lái xe là bắt buộc")]
        [StringLength(12, MinimumLength = 10, ErrorMessage = "Số giấy phép phải từ 10-12 ký tự")]
        public string LicenseId { get; set; }

        public string FullName { get; set; }

        [Required(ErrorMessage = "Loại bằng lái là bắt buộc")]
        [StringLength(10, ErrorMessage = "Loại bằng lái không được vượt quá 10 ký tự")]
        [RegularExpression(@"^(A1|A2|A3|A4|B1|B2|C|D|E|F|FB2|FC)$",
            ErrorMessage = "Loại bằng lái không hợp lệ")]
        public string LicenseType { get; set; }

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        [StringLength(50, ErrorMessage = "Trạng thái không được vượt quá 50 ký tự")]
        public string Status { get; set; }

        [Required(ErrorMessage = "Ngày đăng ký là bắt buộc")]
        public DateOnly RegisterDate { get; set; }

        [Required(ErrorMessage = "Nơi đăng ký là bắt buộc")]
        [StringLength(200, ErrorMessage = "Nơi đăng ký không được vượt quá 200 ký tự")]
        public string RegisterOffice { get; set; }

        public bool IsApproved { get; set; }

        public DateTime DateCreated { get; set; }

        public ICollection<Image> Images { get; set; } = new List<Image>();

        public DriverLicense() { }

        public DriverLicense(int userId, DateTime dateCreated, string status, string licenseId,
                            string licenseType, bool isApproved, DateOnly registerDate,
                            string registerOffice)
        {
            UserId = userId;
            LicenseId = licenseId;
            LicenseType = licenseType;
            Status = status;
            RegisterDate = registerDate;
            RegisterOffice = registerOffice;
            DateCreated = dateCreated;
            IsApproved = isApproved;
        }
    }
}