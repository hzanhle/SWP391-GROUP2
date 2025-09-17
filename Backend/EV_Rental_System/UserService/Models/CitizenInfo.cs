using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace UserService.Models
{
    public class CitizenInfo
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public string CitizenId { get; set; }

        public string Sex { get; set; }

        public DateOnly CitiRegisDate { get; set; }

        public string CitiRegisOffice { get; set; }

        public string FullName { get; set; }
        public string Address { get; set; }
        public DateOnly DayOfBirth { get; set; }
        public string[]? ImageUrls { get; set; }

        public CitizenInfo()
        {
        }

        public CitizenInfo(int id, int userId, string sex, string citizenId, DateOnly citiRegisDate, string citiRegisOffice, string fullName, string address, DateOnly dayOfBirth, string[] imageUrls)
        {
            Id = id;
            Sex = sex;
            UserId = userId;
            CitizenId = citizenId;
            CitiRegisDate = citiRegisDate;
            CitiRegisOffice = citiRegisOffice;
            FullName = fullName;
            Address = address;
            DayOfBirth = dayOfBirth;
            ImageUrls = imageUrls;
        }
    }
}
