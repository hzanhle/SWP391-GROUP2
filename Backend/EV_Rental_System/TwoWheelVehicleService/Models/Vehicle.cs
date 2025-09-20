using System.ComponentModel.DataAnnotations;

namespace TwoWheelVehicleService.Models
{
    public class Vehicle
    {
        [Key]
        public int VehicleId { get; set; }
        public int ModelId { get; set; }
        public Model Model { get; set; }
        public string Color { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; }
    }
}
