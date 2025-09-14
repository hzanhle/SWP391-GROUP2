using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace UserService.Models
{
    public class DriverLicense
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public string LicenseId { get; set; }
        [Required]
        public string LicenseType { get; set; }
        [Required]
        public DateOnly RegisterDate { get; set; }
        [Required]
        public string RegisterOffice { get; set; }

        public DriverLicense()
        {

        }
        public DriverLicense(int id, int userId, string licenseId, string licenseType, DateOnly registerDate, string registerOffice)
        {
            Id = id;
            UserId = userId;
            LicenseId = licenseId;
            LicenseType = licenseType;
            RegisterDate = registerDate;
            RegisterOffice = registerOffice;
        }
    }
}
