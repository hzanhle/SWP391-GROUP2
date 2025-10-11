using System.ComponentModel.DataAnnotations;
using UserService.Models.UserService.Models;

namespace UserService.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên người dùng là bắt buộc")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Tên người dùng phải từ 3-100 ký tự")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(150, ErrorMessage = "Email không được vượt quá 150 ký tự")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [RegularExpression(@"^0[0-9]{9}$", ErrorMessage = "Số điện thoại phải bắt đầu bằng 0 và có 10 chữ số")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6-100 ký tự")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public ICollection<CitizenInfo>? CitizenInfos { get; set; }
        public ICollection<DriverLicense>? DriverLicenses { get; set; }
        public ICollection<Notification>? Notifications { get; set; }

        public DateTime CreatedAt { get; set; }

        //[Required(ErrorMessage = "RoleId là bắt buộc")]
        //[Range(1, int.MaxValue, ErrorMessage = "RoleId phải lớn hơn 0")]
        public int RoleId { get; set; }

        public Role? Role { get; set; }

        public bool IsActive { get; set; } = true;

        public User() { }

        public User(int id, string? userName, string? email, string? phoneNumber,
                    string? password, DateTime createdAt)
        {
            Id = id;
            UserName = userName;
            Email = email;
            PhoneNumber = phoneNumber;
            Password = password;
            CreatedAt = createdAt;
        }
    }
}
