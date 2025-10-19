namespace BookingService.DTOs
{
    public class UserInformation
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string Dob { get; set; } // Date of Birth in string format (e.g., "DD-MM-YYYY")
        public string CitizenId { get; set; }
        public string Address { get; set; }
    }
}
