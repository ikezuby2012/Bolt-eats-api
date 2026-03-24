using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Auth.ResendOtp;

internal sealed class ResendOtpCommandHandler(IApplicationDbContext context, IOtpHandler otpHandler) : ICommandHandler<ResendOtpCommand, Guid>
{
    public async Task<Result<Guid>> Handle(ResendOtpCommand command, CancellationToken cancellationToken)
    {
        User? user = await context.Users.SingleOrDefaultAsync(u => u.Email == command.email, cancellationToken);

        if (user == null)
        {
            return Result.Failure<Guid>(UserErrors.NotFoundByEmail);
        }

        // generate otp
        string newOtp = otpHandler.GenerateOtp();

        user.OTP = newOtp;

        await context.SaveChangesAsync(cancellationToken);

        // send email to user
        user.Raise(new UserRegisteredDomainEvent(user, newOtp));

        return user.Id;
    }
}

