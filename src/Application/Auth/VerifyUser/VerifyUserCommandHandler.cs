using System.Globalization;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Auth.Dto;
using Application.Users.Dto;
using Domain.Auth;
using Domain.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SharedKernel;

namespace Application.Auth.VerifyUser;

internal sealed class VerifyUserCommandHandler(IUnitOfWork unitOfWork, IApplicationDbContext db,
    ITokenProvider tokenProvider,
    IHttpContextAccessor httpContextAccessor,
    IDateTimeProvider dateTimeProvider,
    IConfiguration configuration) : ICommandHandler<VerifyUserCommand, LoginSuccessDto>
{
    public string? ClientIP
    {
        get
        {
            System.Net.IPAddress? remoteIp = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress;
            return remoteIp?.ToString();
        }
    }

    public async Task<Result<LoginSuccessDto>> Handle(VerifyUserCommand command, CancellationToken cancellationToken)
    {
        User? user = await unitOfWork.UserRepository.SingleOrDefaultAsync(u => u.Email == command.email, cancellationToken);

        if (user == null)
        {
            return Result.Failure<LoginSuccessDto>(UserErrors.NotFoundByEmail);
        }

        // verify otp
        if (user.OTP != command.otp)
        {
            return Result.Failure<LoginSuccessDto>(AuthError.InvalidOtp);
        }

        user.isVerifed = true;
        user.OTP = ""; // remove otp


        string token = tokenProvider.Create(user);
        string refreshToken = tokenProvider.GenerateRefreshToken();

        // save refresh token
        string refreshExpiryDate = configuration["Jwt:RefreshTokenExpiryDays"] ?? "14";
        double refreshDays = Convert.ToDouble(refreshExpiryDate, CultureInfo.InvariantCulture);
        db.RefreshTokens.Add(new RefreshToken
        {
            CreatedAt = dateTimeProvider.UtcNow,
            CreatedBy = user.Id.ToString(),
            CreatedByIp = ClientIP ?? "",
            ExpiresAt = dateTimeProvider.UtcNow.AddDays(refreshDays),
            IsSoftDeleted = false,
            Token = refreshToken,
        });

        unitOfWork.UserRepository.Update(user);

        await db.SaveChangesAsync(cancellationToken);

        var userRes = (CreatedUserDto)user;

        return new LoginSuccessDto { Token = token, RefreshToken = refreshToken, User = userRes };
    }
}
