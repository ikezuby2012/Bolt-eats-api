using System.Security.Claims;
using Domain.Users;

namespace Application.Abstractions.Authentication;

public interface ITokenProvider
{
    string Create(User user);
    string GenerateRefreshToken();
    Task<ClaimsPrincipal?> GetPrincipalFromExpiredToken(string token);
}
