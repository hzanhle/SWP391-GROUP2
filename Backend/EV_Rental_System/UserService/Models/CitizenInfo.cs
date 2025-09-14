using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace UserService.Models
{
    public class CitizenInfo
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public string CitizenId { get; set; }
        [Required]
        public string Sex { get; set; }
        [Required]
        public DateOnly CitiRegisDate { get; set; }
        [Required]
        public string CitiRegisOffice { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public DateOnly DayOfBirth { get; set; }

        public CitizenInfo()
        {
        }

        public CitizenInfo(int id, int userId, string sex ,string citizenId, DateOnly citiRegisDate, string citiRegisOffice, string fullName, string address, DateOnly dayOfBirth)
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
        }
    }
}
