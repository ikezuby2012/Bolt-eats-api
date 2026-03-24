using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Abstractions.Authentication;
using Domain.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Authentication;

internal sealed class TokenProvider(IConfiguration configuration) : ITokenProvider
{
    public string Create(User user)
    {
        string secretKey = configuration["Jwt:Secret"]!;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            ]),
            Expires = DateTime.UtcNow.AddMinutes(configuration.GetValue<int>("Jwt:ExpirationInMinutes")),
            SigningCredentials = credentials,
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"]
        };

        var handler = new JsonWebTokenHandler();

        string token = handler.CreateToken(tokenDescriptor);

        return token;
    }

    public string GenerateRefreshToken()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public async Task<ClaimsPrincipal?> GetPrincipalFromExpiredToken(string token)
    {
        string secretKey = configuration["Jwt:Secret"]!;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = securityKey,
            ClockSkew = TimeSpan.Zero,
            LifetimeValidator = (notBefore, expires, securityToken, parameters) =>
            {
                if (notBefore != null && notBefore > DateTime.UtcNow)
                {
                    return false;   // token not yet valid — reject
                }

                if (expires == null)
                {
                    return false;   // no expiry set at all — reject
                }

                // Must have been a real token (not expired before it was even issued)
                if (expires < notBefore)
                {
                    return false;
                }

                // Allow expired — this is the refresh flow
                return true;
            }
        };



        TokenValidationResult result = await new JsonWebTokenHandler().ValidateTokenAsync(token, validationParams);

        if (!result.IsValid)
        {
            return null;
        }

        if (result.SecurityToken is not JsonWebToken jwt ||
            !jwt.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return new ClaimsPrincipal(result.ClaimsIdentity);
    }
}
