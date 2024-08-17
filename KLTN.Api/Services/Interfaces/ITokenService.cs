using KLTN.Domain.Entities;
using System.Security.Claims;

namespace KLTN.Api.Services.Interfaces
{
    public interface ITokenService
    {
        Task<string> GenerateTokens(User user,DateTime expiresAt);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetTokenPrincipal(string token);
    }
}
