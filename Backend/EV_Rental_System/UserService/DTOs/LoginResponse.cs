using UserService.Models;

namespace UserService.DTOs
{
    public class LoginResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string? Token { get; set; }
        public string? TokenType { get; set; }
        public int ExpiresIn { get; set; }
        public UserDTO? User { get; set; }
    }
}
