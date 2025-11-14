namespace BookingService.DTOs
{
    /// <summary>
    /// DTO for displaying risk customer information in list view
    /// </summary>
    public class RiskCustomerDTO
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        
        // Trust Score
        public int TrustScore { get; set; }
        
        // Risk Assessment
        public int RiskScore { get; set; } // 0-100 (calculated)
        public string RiskLevel { get; set; } = string.Empty; // "Low", "Medium", "High", "Critical"
        public List<string> RiskFactors { get; set; } = new();
        
        // Statistics
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int LateReturnsCount { get; set; }
        public int DamageCount { get; set; }
        public decimal TotalDamageAmount { get; set; }
        public int NoShowCount { get; set; }
        public int PenaltyCount { get; set; }
        
        // Timestamps
        public DateTime? LastOrderDate { get; set; }
        public DateTime? LastViolationDate { get; set; }
    }

    /// <summary>
    /// DTO for detailed risk customer profile
    /// </summary>
    public class UserRiskProfileDTO : RiskCustomerDTO
    {
        // Detailed violation history
        public List<ViolationDetailDTO> Violations { get; set; } = new();
        
        // Recent orders with issues
        public List<OrderRiskInfoDTO> RecentOrders { get; set; } = new();
        
        // Trust score history summary
        public List<TrustScoreChangeDTO> RecentScoreChanges { get; set; } = new();
    }

    /// <summary>
    /// DTO for violation details
    /// </summary>
    public class ViolationDetailDTO
    {
        public int OrderId { get; set; }
        public string ViolationType { get; set; } = string.Empty; // "LateReturn", "Damage", "NoShow"
        public string Description { get; set; } = string.Empty;
        public decimal? Amount { get; set; }
        public DateTime ViolationDate { get; set; }
    }

    /// <summary>
    /// DTO for order risk information
    /// </summary>
    public class OrderRiskInfoDTO
    {
        public int OrderId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool HasLateReturn { get; set; }
        public bool HasDamage { get; set; }
        public decimal? DamageCharge { get; set; }
        public decimal? OvertimeHours { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO for trust score change summary
    /// </summary>
    public class TrustScoreChangeDTO
    {
        public int ChangeAmount { get; set; }
        public string ChangeType { get; set; } = string.Empty; // "Bonus", "Penalty"
        public string Reason { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int? OrderId { get; set; }
    }
}

