namespace BookingService.DTOs
{
    public class VehicleDto
    {
        public int VehicleId { get; set; }
        public string ModelName { get; set; }      // Ví dụ: "VinFast VF8"
        public string LicensePlate { get; set; } // Ví dụ: "51K-123.45"
        public string Type { get; set; }       // Ví dụ: "Sedan", "SUV"
        public string Color { get; set; }      // Ví dụ: "Trắng"
    }
}
