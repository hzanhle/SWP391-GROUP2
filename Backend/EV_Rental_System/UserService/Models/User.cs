using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace UserService.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string Password { get; set; }

        // Navigation properties (quan hệ 1-1, optional)
        public CitizenInfo? CitizenInfo { get; set; }
        public DriverLicense? DriverLicense { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public int RoleId { get; set; }
        public Role Role { get; set; }

        public bool IsActive { get; set; } = true;

        public User() { }

        public User(int id, string userName, string email, string phoneNumber, string password, DateTime createdAt)
        {
            Id = id;
            UserName = userName;
            Email = email;
            PhoneNumber = phoneNumber;
            Password = password;
            CreatedAt = createdAt;
            IsActive = true;
        }

        // Constructor có profile (nullable an toàn hơn)
        public User(int id, string userName, string email, string phoneNumber, string password,
                    CitizenInfo? citizenInfo, DriverLicense? driverLicense, DateTime createdAt)
        {
            Id = id;
            UserName = userName;
            Email = email;
            PhoneNumber = phoneNumber;
            Password = password;
            CitizenInfo = citizenInfo;
            DriverLicense = driverLicense;
            CreatedAt = createdAt;
            IsActive = true;
        }
    }

}
