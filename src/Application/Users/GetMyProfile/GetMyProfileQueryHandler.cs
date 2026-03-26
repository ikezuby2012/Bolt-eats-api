using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Users.Dto;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.GetMyProfile;

internal sealed class GetMyProfileQueryHandler(IUserContext userContext, IApplicationDbContext context) : IQueryHandler<GetMyProfileQuery, UserDto>
{
    public async Task<Result<UserDto>> Handle(GetMyProfileQuery query, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        User? user = await context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserDto>(UserErrors.NotFoundByEmail);
        }

        return (UserDto)user;
    }
}
