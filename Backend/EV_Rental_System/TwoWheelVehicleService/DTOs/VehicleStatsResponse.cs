namespace TwoWheelVehicleService.DTOs
{
    public class VehicleStatsResponse
    {
        public int Total { get; set; }
        public int Available { get; set; }
        public int InUse { get; set; }
        public int Charging { get; set; }
        public int NeedsAttention { get; set; }
    }
}
