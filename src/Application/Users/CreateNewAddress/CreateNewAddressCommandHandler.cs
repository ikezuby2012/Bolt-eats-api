using System.Globalization;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Users.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.CreateNewAddress;
internal sealed class CreateNewAddressCommandHandler(IApplicationDbContext context, IUserContext userContext, IDateTimeProvider dateTimeProvider) : ICommandHandler<CreateNewAddressCommand, AddressDto>
{
    public async Task<Result<AddressDto>> Handle(CreateNewAddressCommand command, CancellationToken cancellationToken)
    {
        decimal latitude = decimal.Parse(command.LatitudeRaw, CultureInfo.InvariantCulture);
        decimal longitude = decimal.Parse(command.LongitudeRaw, CultureInfo.InvariantCulture);

        Guid userId = userContext.UserId;

        if (command.IsDefault)
        {
            await context.Addresses
                .Where(a => a.UserId == userId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(a => a.IsDefault, false),
                    cancellationToken);
        }


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
            Location = Domain.Address.Address.CreatePoint((double)latitude, (double)longitude),
            IsDefault = command.IsDefault,
            CreatedAt = dateTimeProvider.UtcNow,
            CreatedBy = userId.ToString(),
            BuildingDetails = command.BuildingDetails,
            DeliveryInstructions = command.DeliveryInstructions,
            AddressLabel = command.AddressLabel,
            BuildingType = command.BuildingType,
            LatitudeRaw = command.LatitudeRaw,
            LongitudeRaw = command.LongitudeRaw

        };

        await context.Addresses.AddAsync(address, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return (AddressDto)address;
    }
}
