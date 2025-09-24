using System.ComponentModel.DataAnnotations;

namespace UserService.Models
{
    public class DriverLicense
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string LicenseId { get; set; }
        public string LicenseType { get; set; }
        public DateOnly RegisterDate { get; set; }
        public string RegisterOffice { get; set; }

        // Navigation property → liên kết với bảng Image
        public ICollection<Image> Images { get; set; } = new List<Image>();

        public DriverLicense() { }

        public DriverLicense(int userId, string licenseId, string licenseType, DateOnly registerDate, string registerOffice)
        {
            UserId = userId;
            LicenseId = licenseId;
            LicenseType = licenseType;
            RegisterDate = registerDate;
            RegisterOffice = registerOffice;
        }
    }
}