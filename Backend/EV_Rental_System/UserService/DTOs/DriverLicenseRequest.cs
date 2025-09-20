namespace UserService.DTOs
{
    public class DriverLicenseRequest
    {
        public int UserId { get; set; }

        public string LicenseId { get; set; }

        public string LicenseType { get; set; }

        public DateOnly RegisterDate { get; set; }

        public string RegisterOffice { get; set; }

        public string[] ImageUrls { get; set; }
    }
}
