namespace TwoWheelVehicleService.Models
{
    public class TransferVehicle
    {
        public int VehicleId { get; set; } // Vehicle chuyển đi

        public int CurrentStationId { get; set; } // Station hiện tại của Vehicle

        public int ModelId { get; set; } // Model của Vehicle

        public int TargetStationId { get; set; } // Station đích của Vehicle

        public string TransferStatus { get; set; } = string.Empty; // Trạng thái chuyển xe (Đang chuyển, Hoàn thành, Hủy bỏ, v.v.)

        public DateTime CreateAt { get; set; } // Thời gian tạo lệnh chuyển xe

        public DateTime? UpdateAt { get; set; } // Thời gian cập nhật lệnh chuyển xe gần nhất
    }
}
