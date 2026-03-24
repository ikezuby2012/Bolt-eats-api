using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Users.Dto;
using Domain.Auth;
using Domain.Users;
using SharedKernel;

namespace Application.Auth.VerifyUser;

internal sealed class VerifyUserCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<VerifyUserCommand, CreatedUserDto>
{
    public async Task<Result<CreatedUserDto>> Handle(VerifyUserCommand command, CancellationToken cancellationToken)
    {
        User? user = await unitOfWork.UserRepository.SingleOrDefaultAsync(u => u.Email == command.email, cancellationToken);

        if (user == null)
        {
            return Result.Failure<CreatedUserDto>(UserErrors.NotFoundByEmail);
        }

        // verify otp
        if (user.OTP != command.otp)
        {
            return Result.Failure<CreatedUserDto>(AuthError.InvalidOtp);
        }

        user.isVerifed = true;
        user.OTP = ""; // remove otp

        unitOfWork.UserRepository.Update(user);
        await unitOfWork.SaveAsync(cancellationToken);

        return (CreatedUserDto)user;
    }
}
