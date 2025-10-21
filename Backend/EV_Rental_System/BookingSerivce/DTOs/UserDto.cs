namespace BookingService.DTOs
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string CitizenId { get; set; }
        public string Address { get; set; }
        public string DateOfBirth { get; set; } // Format: "DD-MM-YYYY"
    }
}
