using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Users.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.SetAddressAsDefault;
internal sealed class SetAddressAsDefaultCommandHandler(IUserContext userContext, IApplicationDbContext context) : ICommandHandler<SetAddressAsDefaultCommand, AddressDto>
{
    public async Task<Result<AddressDto>> Handle(SetAddressAsDefaultCommand command, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        List<Domain.Address.Address> addresses = await context.Addresses
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        if (!addresses.Any())
        {
            return Result.Failure<AddressDto>(Domain.Common.CommonErrors.CustomErrorMessage("User has no addresses"));
        }

        Domain.Address.Address? addressToSet = addresses.FirstOrDefault(x => x.Id == command.Id);

        if (addressToSet is null)
        {
            return Result.Failure<AddressDto>(Domain.Common.CommonErrors.CustomErrorMessage("Address Not Found"));
        }

        if (addresses.FirstOrDefault(x => x.IsDefault) is { } currentDefault)
        {
            currentDefault.IsDefault = false;
        }

        addressToSet.IsDefault = true;

        await context.SaveChangesAsync(cancellationToken);

        return (AddressDto)addressToSet;
    }
}
