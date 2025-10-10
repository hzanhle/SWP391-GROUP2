using System.ComponentModel.DataAnnotations;

namespace TwoWheelVehicleService.DTOs
{
    public class ModelRequest
    {
        [MaxLength(10, ErrorMessage = "Chỉ được upload tối đa 10 ảnh")]
        public List<IFormFile>? Files { get; set; }

        [Required(ErrorMessage = "Tên mẫu xe không được để trống")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên mẫu xe phải từ 2 đến 100 ký tự")]
        public string ModelName { get; set; }

        [Required(ErrorMessage = "Hãng sản xuất không được để trống")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Tên hãng sản xuất phải từ 2 đến 50 ký tự")]
        public string Manufacturer { get; set; }

        [Range(2000, 2100, ErrorMessage = "Năm sản xuất phải từ 2000 đến 2100")]
        public int Year { get; set; }

        [Range(1, 500, ErrorMessage = "Tốc độ tối đa phải từ 1 đến 500 km/h")]
        public int MaxSpeed { get; set; }

        [Range(1000, 100000, ErrorMessage = "Dung lượng pin phải từ 1000 đến 100000 mAh")]
        public int BatteryCapacity { get; set; }

        [Range(10, 1440, ErrorMessage = "Thời gian sạc phải từ 10 đến 1440 phút (24 giờ)")]
        public int ChargingTime { get; set; }

        [Range(10, 1000, ErrorMessage = "Quãng đường di chuyển phải từ 10 đến 1000 km")]
        public int BatteryRange { get; set; }

        [Range(1, 10, ErrorMessage = "Số chỗ ngồi phải từ 1 đến 10")]
        public int VehicleCapacity { get; set; }

        [Range(1000000, 10000000000, ErrorMessage = "Giá thành phải từ 1,000,000 đến 10,000,000,000 VNĐ")]
        public double ModelCost { get; set; }

        [Range(10000, 10000000, ErrorMessage = "Giá thuê theo giờ phải từ 10,000 đến 10,000,000 VNĐ")]
        public double RentFeeForHour { get; set; }
    }

    // Custom Validation Attribute cho File
    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;

        public AllowedExtensionsAttribute(string[] extensions)
        {
            _extensions = extensions;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is List<IFormFile> files)
            {
                foreach (var file in files)
                {
                    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!_extensions.Contains(extension))
                    {
                        return new ValidationResult($"Chỉ chấp nhận file: {string.Join(", ", _extensions)}");
                    }
                }
            }
            return ValidationResult.Success;
        }
    }
}
