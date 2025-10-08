using System.ComponentModel.DataAnnotations;

namespace UserService.Models
{
    public class Image
    {
        [Key]
        public int ImageId { get; set; }

        [Required]
        public string Url { get; set; }

        [Required]
        public string Type { get; set; }
        [Required]
        public int TypeId { get; set; } // CitizenId hoặc DriverLicenseId

        public Image() { }

        public Image(string url, string type, int typeId)
        {
            Url = url;
            Type = type;
            TypeId = typeId;
        }
    }
}
