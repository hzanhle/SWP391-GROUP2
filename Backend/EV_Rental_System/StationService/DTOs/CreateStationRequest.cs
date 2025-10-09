namespace StationService.DTOs
{
    public class CreateStationRequest
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public int? ManagerId { get; set; } // User với role là Staff
    }
}