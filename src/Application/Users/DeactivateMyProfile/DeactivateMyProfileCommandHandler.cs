using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.DeactivateMyProfile;

internal sealed class DeactivateMyProfileCommandHandler(IUserContext userContext, IApplicationDbContext context, IDateTimeProvider dateTimeProvider) : ICommandHandler<DeactivateMyProfileCommand>
{
    public async Task<Result> Handle(DeactivateMyProfileCommand command, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        User? user = await context.Users.SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFoundByEmail);
        }

        user.UpdatedAt = dateTimeProvider.UtcNow;
        user.IsSoftDeleted = true;
        user.IsActive = false;
        user.UpdatedBy = userId.ToString();

        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
