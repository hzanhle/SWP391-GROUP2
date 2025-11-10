using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace UserService.DTOs
{
    public class DriverLicenseRequest
    {
        [Required(ErrorMessage = "Số giấy phép lái xe là bắt buộc")]
        [StringLength(12, MinimumLength = 10, ErrorMessage = "Số giấy phép phải từ 10-12 ký tự")]
        public string LicenseId { get; set; }

        [Required(ErrorMessage = "Loại bằng lái là bắt buộc")]
        [StringLength(10, ErrorMessage = "Loại bằng lái không được vượt quá 10 ký tự")]
        [RegularExpression(@"^(A1|A2|A3|A4|B1|B2|C|D|E|F|FB2|FC)$",
            ErrorMessage = "Loại bằng lái không hợp lệ")]
        public string LicenseType { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2-100 ký tự")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        [RegularExpression(@"^(Nam|Nữ|Khác)$", ErrorMessage = "Giới tính phải là Nam, Nữ hoặc Khác")]
        public string Sex { get; set; }

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        public DateOnly DayOfBirth { get; set; }

        [Required(ErrorMessage = "Ngày đăng ký là bắt buộc")]
        public DateOnly RegisterDate { get; set; }

        [Required(ErrorMessage = "Nơi đăng ký là bắt buộc")]
        [StringLength(200, ErrorMessage = "Nơi đăng ký không được vượt quá 200 ký tự")]
        public string RegisterOffice { get; set; }

        // Nếu FE gửi file nhị phân (form-data)
        public List<IFormFile>? Files { get; set; }
    }
}
