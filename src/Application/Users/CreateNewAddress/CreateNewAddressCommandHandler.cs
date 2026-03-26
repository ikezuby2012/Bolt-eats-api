using System.Globalization;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Users.Dto;
using SharedKernel;

namespace Application.Users.CreateNewAddress;
internal sealed class CreateNewAddressCommandHandler(IApplicationDbContext context, IUserContext userContext, IDateTimeProvider dateTimeProvider) : ICommandHandler<CreateNewAddressCommand, AddressDto>
{
    public async Task<Result<AddressDto>> Handle(CreateNewAddressCommand command, CancellationToken cancellationToken)
    {
        decimal latitude = decimal.Parse(command.LatitudeRaw, CultureInfo.InvariantCulture);
        decimal longitude = decimal.Parse(command.LongitudeRaw, CultureInfo.InvariantCulture);

        Guid userId = userContext.UserId;

        var address = new Domain.Address.Address
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Label = command.Label,
            Street = command.Street,
            City = command.City,
            State = command.State,
            Country = command.Country,
            PostalCode = command.PostalCode,
            Latitude = latitude,
            Longitude = longitude,
            IsDefault = command.IsDefault,
            CreatedAt = dateTimeProvider.UtcNow,
            CreatedBy = userId.ToString(),
        };

        ///if command.isDefault IS TRUE, update everything DEFAULT to false

        await context.Addresses.AddAsync(address, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return (AddressDto)address;
    }
}
