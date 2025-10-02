namespace UserService.DTOs
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public int? StationId { get; set; }
        public string FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }
        
    }

}
