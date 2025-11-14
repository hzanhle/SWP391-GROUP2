using System.ComponentModel.DataAnnotations;

namespace AdminDashboardService.ExternalModels.BookingServiceModels
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public int VehicleId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public int OnlineContractId { get; set; }
        // PaymentId removed - Order now has one-to-many relationship with Payments via OrderId foreign key

        public decimal HourlyRate { get; set; }
        public decimal TotalCost { get; set; }
        public decimal DepositAmount { get; set; }
        public int InitialTrustScore { get; set; }

        public string Status { get; set; } = "Pending"; // Stored as string in DB (from enum)
        public DateTime? ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public Payment? Payment { get; set; }
        public OnlineContract? OnlineContract { get; set; }
    }

    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Store as string: "Pending", "Completed", "Failed", "Refunded"
        
        public string? TransactionId { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? PaymentGatewayResponse { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class OnlineContract
    {
        [Key]
        public int ContractId { get; set; }
        public int OrderId { get; set; }
        public string Terms { get; set; } = string.Empty;
        public string ContractNumber { get; set; } = string.Empty;
        public string Status { get; set; } = "Draft";
        public DateTime CreatedAt { get; set; }
        public DateTime? SignedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? SignatureData { get; set; }
        public string? SignedFromIpAddress { get; set; }
        public string? PdfFilePath { get; set; }
        public int TemplateVersion { get; set; } = 1;
        public DateTime? UpdatedAt { get; set; }
    }

    public class VehicleInspectionReport
    {
        [Key]
        public int InspectionId { get; set; }
        public int OrderId { get; set; }
        public string InspectionType { get; set; } = string.Empty;
        public DateTime InspectionDate { get; set; }
        public int InspectorId { get; set; }
        public int CurrentMileage { get; set; }
        public int FuelLevel { get; set; }
        public string OverallCondition { get; set; } = "Good";
        public bool HasDamage { get; set; } = false;
        public decimal CompensationAmount { get; set; } = 0;
        public string CompensationStatus { get; set; } = "NotRequired";
        public string? GeneralNotes { get; set; }
        public string? CustomerSignature { get; set; }
        public DateTime? CustomerSignedAt { get; set; }
        public string? InspectorSignature { get; set; }
        public string Status { get; set; } = "Draft";
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class InspectionDetail
    {
        [Key]
        public int DetailId { get; set; }
        public int InspectionId { get; set; }
        public string Category { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public bool HasIssue { get; set; } = false;
        public string? Severity { get; set; }
        public string? IssueDescription { get; set; }
        public bool RequiresCompensation { get; set; } = false;
        public decimal CompensationAmount { get; set; } = 0;
        public string? Location { get; set; }
        public string Status { get; set; } = "OK";
        public DateTime CreatedAt { get; set; }
    }

    public class InspectionImage
    {
        [Key]
        public int ImageId { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public string ImageType { get; set; } = "Detail";
        public int InspectionId { get; set; }
        public int? DetailId { get; set; }
    }

    public class Notification
    {
        [Key]
        public int? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public int? DataId { get; set; }
        public int? StaffId { get; set; }
        public int UserId { get; set; }
        public DateTime Created { get; set; }
    }
}