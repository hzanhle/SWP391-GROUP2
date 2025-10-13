namespace UserService.DTOs
{
    public class UserDetailDTO
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string UserName { get; set; }
        public DriverLicenseDTO? DriverLicense { get; set; }
        public CitizenInfoDTO? CitizenInfo { get; set; }
    }
}
