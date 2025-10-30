namespace TwoWheelVehicleService.DTOs
{
    public class ResponseDTO
    {
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
        public object Data { get; set; }
    }
}
