using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TwoWheelVehicleService.Models
{

    public class Model
    {
        [Key]
        public int ModelId { get; set; }

        [Required(ErrorMessage = "Tên mẫu xe không được để trống")]
        [StringLength(100, ErrorMessage = "Tên mẫu xe không được vượt quá 100 ký tự")]
        public string ModelName { get; set; }

        [Required(ErrorMessage = "Hãng sản xuất không được để trống")]
        [StringLength(50, ErrorMessage = "Tên hãng sản xuất không được vượt quá 50 ký tự")]
        public string Manufacturer { get; set; }

        [Range(1900, 2100, ErrorMessage = "Năm sản xuất phải từ 1900 đến 2100")]
        public int Year { get; set; }

        [Range(1, 500, ErrorMessage = "Tốc độ tối đa phải từ 1 đến 500 km/h")]
        public int MaxSpeed { get; set; }

        [Range(1, 100000, ErrorMessage = "Dung lượng pin phải từ 1 đến 100000 mAh")]
        public int BatteryCapacity { get; set; }

        [Range(1, 1440, ErrorMessage = "Thời gian sạc phải từ 1 đến 1440 phút")]
        public int ChargingTime { get; set; }

        [Range(1, 1000, ErrorMessage = "Quãng đường di chuyển phải từ 1 đến 1000 km")]
        public int BatteryRange { get; set; }

        [Range(1, 10, ErrorMessage = "Số chỗ ngồi phải từ 1 đến 10")]
        public int VehicleCapacity { get; set; }

        public bool IsActive { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá thành phải lớn hơn hoặc bằng 0")]
        public double ModelCost { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá thuê theo giờ phải lớn hơn hoặc bằng 0")]
        public double RentFeeForHour { get; set; }

        // Navigation properties
        public ICollection<Image> Images { get; set; } = new List<Image>();

        [JsonIgnore]

        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

        public Model() { }

        public Model(string modelName, string manufacturer, int year, int maxSpeed,
                     int batteryCapacity, int chargingTime, int batteryRange,
                     int vehicleCapacity, bool isActive, double modelCost, double rentFeeForHour)
        {
            ModelName = modelName;
            Manufacturer = manufacturer;
            Year = year;
            MaxSpeed = maxSpeed;
            BatteryCapacity = batteryCapacity;
            ChargingTime = chargingTime;
            BatteryRange = batteryRange;
            VehicleCapacity = vehicleCapacity;
            IsActive = isActive;
            RentFeeForHour = rentFeeForHour;
            ModelCost = modelCost;
        }
    }

}