using System.ComponentModel.DataAnnotations;

namespace UserService.DTOs
{
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "UserId là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId phải lớn hơn 0")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Mật khẩu cũ là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu cũ phải từ 6-100 ký tự")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu mới phải từ 6-100 ký tự")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp với mật khẩu mới")]
        public string ConfirmPassword { get; set; }
    }
}
