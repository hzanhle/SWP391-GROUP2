using System.ComponentModel.DataAnnotations;

namespace StationService.Models
{
    public class Station
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public int? ManagerId { get; set; } // User với role là Staff
        public bool IsActive { get; set; }
        public ICollection<StaffShift> StaffShifts { get; set; }
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
