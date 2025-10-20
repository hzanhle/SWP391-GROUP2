namespace BookingSerivce.DTOs
{
    /// <summary>
    /// Role-based order status response.
    /// Different fields are populated based on user role.
    /// </summary>
    public class OrderStatusResponse
    {
        public int OrderId { get; set; }
        public string InternalStatus { get; set; } = string.Empty;
        public string DisplayStatus { get; set; } = string.Empty;
        public bool CanModifyStatus { get; set; }
        public List<string> AvailableActions { get; set; } = new();

        // Scheduled times
        public DateTime? ScheduledPickupTime { get; set; }
        public DateTime? ScheduledReturnTime { get; set; }

        // Actual times (for admin/staff)
        public DateTime? ActualPickupTime { get; set; }
        public DateTime? ActualReturnTime { get; set; }

        // Vehicle tracking (for admin/staff)
        public int? PickupOdometerReading { get; set; }
        public int? ReturnOdometerReading { get; set; }
        public int? PickupBatteryLevel { get; set; }
        public int? ReturnBatteryLevel { get; set; }

        // Staff info (for admin/staff)
        public int? HandedOverByStaffId { get; set; }
        public int? ReceivedByStaffId { get; set; }

        // Inspection info (for admin/staff)
        public bool? HasDamage { get; set; }
        public decimal? DamageCharge { get; set; }

        // Late return info
        public bool IsLateReturn { get; set; }
        public int? LateReturnHours { get; set; }
        public decimal? LateFee { get; set; }

        // Customer-relevant info
        public string? StationName { get; set; }
        public string? StationAddress { get; set; }
        public string? StationPhone { get; set; }
    }
}
