﻿namespace TwoWheelVehicleService.DTOs
{
    public class VehicleStatusResponse
    {
        public long VehicleId { get; set; }
        public string VehicleCode { get; set; } = string.Empty;
        public string VehicleName { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int BatteryLevel { get; set; }
        public string TechnicalStatus { get; set; } = string.Empty;
        public string TechnicalStatusDescription { get; set; } = string.Empty;
        public string? NextBooking { get; set; }
        public string LastUpdate { get; set; } = string.Empty;
        public string LastUpdatedBy { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
