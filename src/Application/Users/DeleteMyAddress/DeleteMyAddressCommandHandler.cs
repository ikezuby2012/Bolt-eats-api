using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.DeleteMyAddress;
internal sealed class DeleteMyAddressCommandHandler(IUserContext userContext, IApplicationDbContext context) : ICommandHandler<DeleteMyAddressCommand>
{
    public async Task<Result> Handle(DeleteMyAddressCommand command, CancellationToken cancellationToken)
    {
        Domain.Address.Address? address = await context.Addresses.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        Guid userId = userContext.UserId;

        if (address == null)
        {
            return Result.Failure(Domain.Common.CommonErrors.CustomErrorMessage("Address was not found!"));
        }

        if (address.UserId != userId)
        {
            return Result.Failure(Domain.Common.CommonErrors.CustomErrorMessage("You cannot delete an address that doesn't belong to you!"));
        }

        bool wasDefault = address.IsDefault;

        if (wasDefault)
        {
            Domain.Address.Address? newDefaultAddress = await context.Addresses
                .Where(a => a.UserId == userId)
                .OrderBy(a => a.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (newDefaultAddress != null)
            {
                newDefaultAddress.IsDefault = true;
                context.Addresses.Update(newDefaultAddress);
            }
        }

        return Result.Success();
    }
}
