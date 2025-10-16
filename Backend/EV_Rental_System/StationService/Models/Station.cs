using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace StationService.Models
{
    public class Station
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Location { get; set; } = null!;
        public int? ManagerId { get; set; } // User với role là Employee
        public bool IsActive { get; set; }
        public ICollection<StaffShift>? StaffShifts { get; set; } = new List<StaffShift>();
        public ICollection<Feedback>? Feedbacks { get; set; } = new List<Feedback>();
        public Station() { }

        public Station(int id, string name, string location, int? managerId, bool isActive)
        {
            Id = id;
            Name = name;
            Location = location;
            ManagerId = managerId;
            IsActive = isActive;
        }
    }
}
