namespace UserService.DTOs
{
    public class CitizenInfoDTO
    {
        public int UserId { get; set; }

        public int Id { get; set; }
        public string CitizenId { get; set; }

        public string Sex { get; set; }

        public DateOnly CitiRegisDate { get; set; }

        public string CitiRegisOffice { get; set; }

        public string FullName { get; set; }

        public string Address { get; set; }

        public DateOnly DayOfBirth { get; set; }

        public string Status { get; set; }

        public List<string> ImageUrls { get; set; }
    }
}
