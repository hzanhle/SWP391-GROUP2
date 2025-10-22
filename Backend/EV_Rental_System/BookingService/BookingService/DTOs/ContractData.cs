namespace BookingSerivce.DTOs
{
    /// <summary>
    /// DTO containing all data needed for contract PDF generation.
    /// All data is fetched from database (Order, User, Vehicle, Payment) for security.
    /// </summary>
    public class ContractData
    {
        // Contract metadata
        public string ContractNumber { get; set; } = string.Empty;
        public DateTime ContractDate { get; set; }

        // User information
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserPhone { get; set; } = string.Empty;
        public string UserIdCard { get; set; } = string.Empty;
        public string UserAddress { get; set; } = string.Empty;

        // Vehicle information
        public string VehicleBrand { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string VehiclePlateNumber { get; set; } = string.Empty;
        public string VehicleColor { get; set; } = string.Empty;
        public decimal HourlyRate { get; set; }

        // Rental period
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalDays { get; set; }

        // Financial details
        public decimal TotalCost { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal DepositPercentage { get; set; }

        // Payment information
        public string TransactionId { get; set; } = string.Empty;
        public decimal PaidAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime PaidAt { get; set; }
    }
}
