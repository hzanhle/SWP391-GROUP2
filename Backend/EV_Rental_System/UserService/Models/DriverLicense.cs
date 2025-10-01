using System.ComponentModel.DataAnnotations;

namespace UserService.Models
{
    public class DriverLicense
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public int LicenseId { get; set; }
        public string LicenseType { get; set; }
        public string Status { get; set; }
        public DateOnly RegisterDate { get; set; }
        public string RegisterOffice { get; set; }

        public bool IsApproved { get; set; }
        public DateTime DateCreated { get; set; }

        // Navigation property → liên kết với bảng Image
        public ICollection<Image> Images { get; set; } = new List<Image>();

        public DriverLicense() { }

        public DriverLicense(int userId, DateTime dateCreated, string status, int licenseId, string licenseType, bool isApproved, DateOnly registerDate, string registerOffice)
        {
            UserId = userId;
            LicenseId = licenseId;
            LicenseType = licenseType;
            Status = status;
            RegisterDate = registerDate;
            RegisterOffice = registerOffice;
            DateCreated = dateCreated;
        }
    }
}