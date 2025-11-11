namespace TwoWheelVehicleService.DTOs
{
    public class TransferRequest
    {
        public List<int> VehicleIds { get; set; } = new List<int>();
        public int ModelId { get; set; }
        public int CurrentStationId { get; set; }
        public int TargetStationId { get; set; }
    }
}
