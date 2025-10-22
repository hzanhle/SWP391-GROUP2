using System.ComponentModel.DataAnnotations;

namespace AdminDashboardService.ExternalModels.UserServiceModels
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int? StationId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int RoleId { get; set; }
        public Role? Role { get; set; }
        public bool IsActive { get; set; }
    }

    public class Role
    {
        [Key]
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }

    public class CitizenInfo
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string CitizenId { get; set; } = string.Empty;
        public string Sex { get; set; } = string.Empty;
        public DateOnly CitiRegisDate { get; set; }
        public string CitiRegisOffice { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateOnly DayOfBirth { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool? IsApproved { get; set; }
        public DateTime DayCreated { get; set; }
    }

    public class DriverLicense
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string LicenseId { get; set; } = string.Empty;
        public string LicenseType { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateOnly RegisterDate { get; set; }
        public string RegisterOffice { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
        public DateTime DateCreated { get; set; }
    }

    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime Created { get; set; }
    }

    public class Image
    {
        [Key]
        public int ImageId { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int TypeId { get; set; }
    }
}