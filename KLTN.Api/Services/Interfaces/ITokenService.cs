using KLTN.Domain.Entities;
using System.Security.Claims;

namespace KLTN.Api.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateTokens(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetTokenPrincipal(string token);
    }
}
