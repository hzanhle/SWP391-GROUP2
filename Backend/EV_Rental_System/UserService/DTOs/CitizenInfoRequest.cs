namespace UserService.DTOs
{
    using Microsoft.AspNetCore.Http;

    public class CitizenInfoRequest
    {
        public string CitizenId { get; set; }
        public string Sex { get; set; }
        public DateOnly CitiRegisDate { get; set; }
        public string CitiRegisOffice { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public DateOnly DayOfBirth { get; set; }
        public List<IFormFile>? Files { get; set; }
    }

}
