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
        public bool IsActive { get; set; }
        public double Lat { get; set; }    // -90..90
        public double Lng { get; set; }    // -180..180
        public ICollection<StaffShift> StaffShifts { get; set; } = new List<StaffShift>();
        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
        public Station() { }

        public Station(int id, string name, string location, bool isActive, double lat = 0, double lng = 0)
        {
            Id = id;
            Name = name;
            Location = location;
            IsActive = isActive;
            Lat = lat;
            Lng = lng;
        }
    }
}
