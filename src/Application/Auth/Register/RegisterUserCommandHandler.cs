using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Users.Dto;
using Domain.Users;
using SharedKernel;

namespace Application.Auth.Register;

internal sealed class RegisterUserCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher, IOtpHandler otpHandler) : ICommandHandler<RegisterUserCommand, CreatedUserDto>
{
    public async Task<Result<CreatedUserDto>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        if (await unitOfWork.UserRepository.AnyAsync(u => u.Email == command.Email, cancellationToken))
        {
            return Result.Failure<CreatedUserDto>(UserErrors.EmailNotUnique);
        }

        string userOtp = otpHandler.GenerateOtp();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            PasswordHash = passwordHasher.Hash(command.Password),
            PhoneNumber = command.phone,
            OTP = userOtp,
            IsSoftDeleted = false,
            IsActive = true,
            isVerifed = false,
            RoleId = UserRole.User.Id,
            CreatedAt = DateTime.UtcNow,
        };

        // send email to new user
        user.Raise(new UserRegisteredDomainEvent(user, userOtp));

        await unitOfWork.UserRepository.AddAsync(user);

        await unitOfWork.SaveAsync(cancellationToken);

        return (CreatedUserDto)user;
    }
}
