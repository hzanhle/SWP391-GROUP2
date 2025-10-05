using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Models;

namespace UserService.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtService> _logger;

        public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public string GenerateToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Tạo claims chứa thông tin user
            var claims = new List<Claim>
        {
            // Standard claims
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            
            // Custom claims - Thông tin cơ bản của user
            new Claim("userId", user.Id.ToString()),
            new Claim("userName", user.UserName),
            new Claim("email", user.Email ?? ""),
            new Claim("phoneNumber", user.PhoneNumber ?? ""),
            new Claim("roleId", user.RoleId.ToString()),
            new Claim("roleName", user.Role?.RoleName ?? "User"),
            new Claim("isActive", user.IsActive.ToString())
        };

            // Tạo token
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtSettings.GetValue<int>("ExpiresInMinutes")),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            _logger.LogInformation("JWT token generated for user: {UserId}", user.Id);

            return tokenString;
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["SecretKey"];

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Token validation failed: {Message}", ex.Message);
                return null;
            }
        }

        public string? GetUserIdFromToken(string token)
        {
            var principal = ValidateToken(token);
            return principal?.FindFirst("userId")?.Value;
        }
    }

}
