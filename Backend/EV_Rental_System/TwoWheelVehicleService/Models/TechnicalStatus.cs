using System.ComponentModel;

namespace TwoWheelVehicleService.Models
{
    public enum TechnicalStatus
    {
        [Description("Tốt - Hoạt động bình thường")]
        Good = 1,

        [Description("Khá - Có tiếng động nhỏ")]
        Fair = 2,

        [Description("Cần kiểm tra - Phát hiện bất thường")]
        NeedsCheck = 3,

        [Description("Cần sửa - Không nên cho thuê")]
        NeedsRepair = 4
    }
}
