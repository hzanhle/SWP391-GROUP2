using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UserService.DTOs
{
    public class CitizenInfoRequest
    {
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

        // File ảnh CCCD mặt trước & mặt sau
        [Required(ErrorMessage = "Phải gửi ít nhất một ảnh CCCD")]
        public List<IFormFile>? Files { get; set; }
    }
}
