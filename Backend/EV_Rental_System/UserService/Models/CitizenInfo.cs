using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UserService.Models
{
    public class CitizenInfo
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public User? User { get; set; }

        public string CitizenId { get; set; }

        public string Sex { get; set; }

        public DateOnly CitiRegisDate { get; set; }

        public string CitiRegisOffice { get; set; }

        public string FullName { get; set; }

        public string Address { get; set; }

        public DateOnly DayOfBirth { get; set; }

        public string Status { get; set; }

        public bool? IsApproved { get; set; }

        public DateTime DayCreated { get; set; }

        public ICollection<Image> Images { get; set; } = new List<Image>();

        public CitizenInfo() { }

        public CitizenInfo(
            int userId,
            string sex,
            string status,
            string citizenId,
            DateOnly citiRegisDate,
            string citiRegisOffice,
            string fullName,
            string address,
            DateOnly dayOfBirth,
            bool isApproved,
            DateTime dayCreated)
        {
            UserId = userId;
            Sex = sex;
            CitizenId = citizenId;
            CitiRegisDate = citiRegisDate;
            CitiRegisOffice = citiRegisOffice;
            FullName = fullName;
            Status = status;
            Address = address;
            DayOfBirth = dayOfBirth;
            IsApproved = isApproved;
            DayCreated = dayCreated;
        }
    }
}
