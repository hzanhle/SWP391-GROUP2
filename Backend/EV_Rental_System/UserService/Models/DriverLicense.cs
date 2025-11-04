using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models
{
    public class DriverLicense
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public User? User { get; set; }

        public string LicenseId { get; set; }

        public string LicenseType { get; set; }

        public string FullName { get; set; }

        public string Sex { get; set; }

        public string Address { get; set; }

        public DateOnly DayOfBirth { get; set; }

        public string Status { get; set; }

        public DateOnly RegisterDate { get; set; }

        public string RegisterOffice { get; set; }

        public bool IsApproved { get; set; }

        public DateTime DateCreated { get; set; }

        public ICollection<Image> Images { get; set; } = new List<Image>();

        public DriverLicense() { }

        public DriverLicense(
            int userId,
            DateTime dateCreated,
            string status,
            string licenseId,
            string address,
            string fullName,
            DateOnly dayOfBirth,
            string licenseType,
            bool isApproved,
            DateOnly registerDate,
            string registerOffice)
        {
            UserId = userId;
            LicenseId = licenseId;
            LicenseType = licenseType;
            Status = status;
            Address = address;
            FullName = fullName;
            DayOfBirth = dayOfBirth;
            RegisterDate = registerDate;
            RegisterOffice = registerOffice;
            DateCreated = dateCreated;
            IsApproved = isApproved;
        }
    }
}
