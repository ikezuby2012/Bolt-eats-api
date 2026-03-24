using System.Globalization;
using System.Security.Claims;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Auth.Dto;
using Application.Users.Dto;
using Domain.Auth;
using Domain.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SharedKernel;

namespace Application.Auth.RefreshTokenAsync;

internal sealed class RefreshTokenCommandHandler(ITokenProvider tokenProvider, IApplicationDbContext context, IDateTimeProvider dateTimeProvider, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : ICommandHandler<RefreshTokenCommand, LoginSuccessDto>
{
    public string? ClientIP
    {
        get
        {
            System.Net.IPAddress? remoteIp = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress;
            return remoteIp?.ToString();
        }
    }

    public async Task<Result<LoginSuccessDto>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        System.Security.Claims.ClaimsPrincipal? principal = await tokenProvider.GetPrincipalFromExpiredToken(command.accessToken);
        if (principal is null)
        {
            return Result.Failure<LoginSuccessDto>(Domain.Common.CommonErrors.CustomErrorMessage("Something went wrong"));
        }

        string? userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userId, out Guid parsedUserId))
        {
            return Result.Failure<LoginSuccessDto>(Domain.Common.CommonErrors.CustomErrorMessage("Invalid User Id Param!"));
        }

        User? user = await context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == parsedUserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<LoginSuccessDto>(Domain.Common.CommonErrors.CustomErrorMessage("User not registered or found!"));
        }

        RefreshToken? storedToken = await context.RefreshTokens.FirstOrDefaultAsync(t =>
                        t.Token == command.refreshToken &&
                        t.CreatedBy == userId &&
                        !t.IsRevoked &&
                        !t.IsUsed &&
                        t.ExpiresAt > dateTimeProvider.UtcNow, cancellationToken: cancellationToken);

        if (storedToken is null)
        {
            return Result.Failure<LoginSuccessDto>(Domain.Common.CommonErrors.CustomErrorMessage("Stored token is missing!"));
        }

        storedToken.IsUsed = true;

        string newAccessToken = tokenProvider.Create(user);
        string newRefreshToken = tokenProvider.GenerateRefreshToken();

        string refreshExpiryDate = configuration["Jwt:RefreshTokenExpiryDays"] ?? "14";
        double refreshDays = Convert.ToDouble(refreshExpiryDate, CultureInfo.InvariantCulture);
        context.RefreshTokens.Add(new RefreshToken
        {
            CreatedAt = dateTimeProvider.UtcNow,
            CreatedBy = user.Id.ToString(),
            CreatedByIp = ClientIP ?? "",
            ExpiresAt = dateTimeProvider.UtcNow.AddDays(refreshDays),
            IsSoftDeleted = false,
            Token = newRefreshToken,
        });

        await context.SaveChangesAsync(cancellationToken);

        var userRes = (CreatedUserDto)user;

        return new LoginSuccessDto { Token = newAccessToken, RefreshToken = newRefreshToken, User = userRes };
    }
}
