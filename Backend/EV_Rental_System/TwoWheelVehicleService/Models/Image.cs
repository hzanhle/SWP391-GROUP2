using System.ComponentModel.DataAnnotations;

namespace TwoWheelVehicleService.Models
{
    public class Image
    {
        [Key]
        public int ImageId { get; set; }

        public string Url { get; set; }

        // Foreign key
        public int ModelId { get; set; }

        // Navigation property
        public Model Model { get; set; }

        public Image() { }

        public Image(int modelId, string url)
        {
            ModelId = modelId;
            Url = url;
        }
    }
}
