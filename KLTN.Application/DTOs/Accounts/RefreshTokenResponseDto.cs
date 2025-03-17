namespace KLTN.Application.DTOs.Accounts
{
    public class RefreshTokenResponseDto
    {
        public string Token { get; set; }
        public DateTime TokenExpiresAt { get; set; }
    }
}
