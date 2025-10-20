using BookingService.Models;

namespace BookingService.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public int VehicleId { get; set; }

        // Thông tin booking
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalDays { get; set; }

        // Thông tin giá (LƯU ĐẦY ĐỦ TẠI ĐÂY)
        public decimal ModelPrice { get; set; } // Giá mẫu xe
        public decimal TotalCost { get; set; }  // Tổng tiền thuê
        public decimal DepositAmount { get; set; } // Tiền cọc yêu cầu (thường 30-50% TotalCost)

        // Trạng thái đơn hàng
        // "Pending" → "PaymentInitiated" → "AwaitingContract" → "ContractSent" → "ContractSigned"
        // → "AwaitingDeposit" → "Deposited" → "Confirmed" → "InProgress" → "Completed" → "Cancelled" → "Expired"
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; } // Order expires if payment not initiated within time limit (5 minutes)

        // Stage 1 Enhancement Fields
        public Guid? PreviewToken { get; set; } // Links to SoftLock that was consumed to create this order
        public int TrustScoreAtBooking { get; set; } // User's trust score at the time of booking (for audit trail)
        public decimal DepositPercentage { get; set; } // Actual deposit percentage used (0.30, 0.40, or 0.50)
        public string? CancellationReason { get; set; } // Why the order was cancelled or expired

        // Stage 2 Enhancement Fields - Contract Generation
        public int? ContractId { get; set; } // Links to generated contract
        public DateTime? ContractGeneratedAt { get; set; } // When contract was auto-generated after payment
        public DateTime? ConfirmedAt { get; set; } // When payment was confirmed

        // Stage 3 Enhancement Fields - Pickup & Return Tracking
        public DateTime? ActualPickupTime { get; set; } // Actual vehicle pickup time (vs scheduled FromDate)
        public int? PickupOdometerReading { get; set; } // Odometer reading at pickup
        public int? PickupBatteryLevel { get; set; } // Battery percentage at pickup (for EV)
        public string? PickupNotes { get; set; } // Staff notes about vehicle condition at pickup
        public int? HandedOverByStaffId { get; set; } // Staff member who handed over vehicle

        public DateTime? ActualReturnTime { get; set; } // Actual vehicle return time (vs scheduled ToDate)
        public int? ReturnOdometerReading { get; set; } // Odometer reading at return
        public int? ReturnBatteryLevel { get; set; } // Battery percentage at return
        public string? ReturnNotes { get; set; } // Staff notes about vehicle condition at return
        public int? ReceivedByStaffId { get; set; } // Staff member who received vehicle

        // Inspection tracking
        public DateTime? InspectionStartedAt { get; set; }
        public DateTime? InspectionCompletedAt { get; set; }
        public int? InspectedByStaffId { get; set; }
        public bool? HasDamage { get; set; }
        public decimal? DamageCharge { get; set; }

        // Late return tracking
        public bool IsLateReturn { get; set; } = false;
        public int? LateReturnHours { get; set; }
        public decimal? LateFee { get; set; }

        // Relationships - Quan hệ 1-1 với Payment và OnlineContract
        public Payment? Payment { get; set; }
        public OnlineContract? OnlineContract { get; set; }

        // Constructors
        public Order()
        {
            CreatedAt = DateTime.UtcNow;
            Status = "Pending";
        }

        public Order(int userId, int vehicleId, DateTime fromDate, DateTime toDate,
                     decimal modelPrice, decimal totalCost, decimal depositAmount)
        {
            UserId = userId;
            VehicleId = vehicleId;
            FromDate = fromDate;
            ToDate = toDate;
            TotalDays = (toDate - fromDate).Days;
            ModelPrice = modelPrice;
            TotalCost = totalCost;
            DepositAmount = depositAmount;
            Status = "Pending";
            CreatedAt = DateTime.UtcNow;
        }

        // Helper methods
        public bool IsValidDateRange()
        {
            return ToDate > FromDate && FromDate >= DateTime.UtcNow.Date;
        }
    }
}