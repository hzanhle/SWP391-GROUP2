using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models
{
    public class DriverLicense
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public string? LicenseId { get; set; }

        public string? LicenseType { get; set; }

        public DateOnly RegisterDate { get; set; }

        public string? RegisterOffice { get; set; }

        public string[]? ImageUrls { get; set; }

        public DriverLicense()
        {
        }

        public DriverLicense(int id, int userId, string? licenseId, string? licenseType, DateOnly registerDate, string? registerOffice, string[]? imageUrls)
        {
            Id = id;
            UserId = userId;
            LicenseId = licenseId;
            LicenseType = licenseType;
            RegisterDate = registerDate;
            RegisterOffice = registerOffice;
            ImageUrls = imageUrls;
        }
    }
}
