using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Users.Dto;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.UpdateMyProfile;

internal sealed class UpdateMyProfileCommandHandler(IUserContext userContext, IApplicationDbContext context, IDateTimeProvider dateTimeProvider) : ICommandHandler<UpdateMyProfileCommand, UserDto>
{
    public async Task<Result<UserDto>> Handle(UpdateMyProfileCommand command, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        User? user = await context.Users.SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserDto>(UserErrors.NotFoundByEmail);
        }

        if (!string.IsNullOrEmpty(command.firstName))
        {
            user.FirstName = command.firstName;
        }
        if (!string.IsNullOrEmpty(command.lastName))
        {
            user.LastName = command.lastName;
        }
        if (!string.IsNullOrEmpty(command.phoneNumber))
        {
            user.PhoneNumber = command.phoneNumber;
        }
        if (command.dateOfBirth.HasValue)
        {
            user.DateOfBirth = command.dateOfBirth.Value;
        }

        user.UpdatedAt = dateTimeProvider.Now;
        user.UpdatedBy = userId.ToString();

        await context.SaveChangesAsync(cancellationToken);

        return (UserDto)user;
    }
}
