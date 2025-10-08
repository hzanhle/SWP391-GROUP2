using System.ComponentModel.DataAnnotations;
using UserService.Models.UserService.Models;

namespace UserService.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        public string? UserName { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Password { get; set; }

        // Quan hệ 1-1 (optional)
        public ICollection<CitizenInfo>? CitizenInfos { get; set; }
        public ICollection<DriverLicense>? DriverLicenses { get; set; }

        public ICollection<Notification>? Notifications { get; set; }

        public DateTime CreatedAt { get; set; }

        public int RoleId { get; set; }

        public Role? Role { get; set; }

        public bool IsActive { get; set; } = true;

        public User() { }

        public User(int id, string? userName, string? email, string? phoneNumber,
                    string? password, DateTime createdAt)
        {
            Id = id;
            UserName = userName;
            Email = email;
            PhoneNumber = phoneNumber;
            Password = password;
            CreatedAt = createdAt;
        }

    }
}
