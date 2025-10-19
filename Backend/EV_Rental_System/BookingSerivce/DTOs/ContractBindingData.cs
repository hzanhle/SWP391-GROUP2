namespace BookingService.DTOs
{
    public class ContractBindingData // Lấy thông tin User và Xe từ FE để binding vào hợp đồng
    {
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerEmail { get; set; }
        public string CitizenId { get; set; } // CMND/CCCD

        // Thông tin Xe (từ FE)
        public string ModelName { get; set; } // "VinFast VF8 Plus"
        public string LicensePlate { get; set; } // "51K-123.45"
        public string VehicleColor { get; set; }
    }
}
