using System.Globalization;
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

namespace Application.Auth.Login;
internal sealed class LoginUserCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider,
    IHttpContextAccessor httpContextAccessor,
    IDateTimeProvider dateTimeProvider,
    IConfiguration configuration
    ) : ICommandHandler<LoginUserCommand, LoginSuccessDto>
{
    public string? ClientIP
    {
        get
        {
            System.Net.IPAddress? remoteIp = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress;
            return remoteIp?.ToString();
        }
    }
    public async Task<Result<LoginSuccessDto>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        User? user = await context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Email == command.Email, cancellationToken);

        if (user is null)
        {
            return Result.Failure<LoginSuccessDto>(UserErrors.NotFoundByEmail);
        }

        bool verified = passwordHasher.Verify(command.Password, user.PasswordHash);

        if (!verified)
        {
            return Result.Failure<LoginSuccessDto>(AuthError.LoginFailed);
        }

        string token = tokenProvider.Create(user);
        string refreshToken = tokenProvider.GenerateRefreshToken();

        // save refresh token
        string refreshExpiryDate = configuration["Jwt:RefreshTokenExpiryDays"] ?? "14";
        double refreshDays = Convert.ToDouble(refreshExpiryDate, CultureInfo.InvariantCulture);
        context.RefreshTokens.Add(new RefreshToken
        {
            CreatedAt = dateTimeProvider.UtcNow,
            CreatedBy = user.Id.ToString(),
            CreatedByIp = ClientIP ?? "",
            ExpiresAt = dateTimeProvider.UtcNow.AddDays(refreshDays),
            IsSoftDeleted = false,
            Token = refreshToken,
        });

        await context.SaveChangesAsync(cancellationToken);

        var userRes = (CreatedUserDto)user;

        return new LoginSuccessDto { Token = token, RefreshToken = refreshToken, User = userRes };
    }
}
