using KLTN.Application.DTOs.Users;

namespace KLTN.Application.DTOs.Accounts
{
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }    
        public DateTime TokenExpiresAt { get; set; }
        public DateTime RefreshTokenExpiresAt { get; set; }
        public string Role { get; set; }
        public UserDto User { get; set; }
    }
}
