namespace UserService.DTOs
{
    public class DriverLicenseDTO
    {
        public int UserId { get; set; }

        public int Id { get; set; }
        public string LicenseId { get; set; }
        public string LicenseType { get; set; }
        public string Status { get; set; }
        public DateOnly RegisterDate { get; set; }
        public string RegisterOffice { get; set; }
        public List<string> ImageUrls { get; set; }
        
    }
}
