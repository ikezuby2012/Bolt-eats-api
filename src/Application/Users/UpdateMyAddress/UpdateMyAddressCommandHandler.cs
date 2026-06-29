using System.Globalization;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Users.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.UpdateMyAddress;

internal sealed class UpdateMyAddressCommandHandler(IApplicationDbContext context, IUserContext userContext, IDateTimeProvider dateTimeProvider) : ICommandHandler<UpdateMyAddressCommand, AddressDto>
{
    public async Task<Result<AddressDto>> Handle(UpdateMyAddressCommand request, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        Domain.Address.Address address = await context.Addresses.FirstOrDefaultAsync(a => a.Id == request.Id && a.UserId == userId, cancellationToken);

        if (address == null)
        {
            return Result.Failure<AddressDto>(Domain.Common.CommonErrors.CustomErrorMessage($"Address with ID {request.Id} not found"));
        }

        if (request.IsDefault)
        {
            await context.Addresses
                .Where(a => a.UserId == userId &&
                            a.Id != request.Id)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(a => a.IsDefault, false),
                    cancellationToken);
        }

        if (!string.IsNullOrEmpty(request.Label))
        {
            address.Label = request.Label;
        }
        if (!string.IsNullOrEmpty(request.Street))
        {
            address.Street = request.Street;
        }
        if (!string.IsNullOrEmpty(request.City))
        {
            address.City = request.City;
        }
        if (!string.IsNullOrEmpty(request.State))
        {
            address.State = request.State;
        }
        if (!string.IsNullOrEmpty(request.Country))
        {
            address.Country = request.Country;
        }
        if (!string.IsNullOrEmpty(request.PostalCode))
        {
            address.PostalCode = request.PostalCode;
        }
        if (!string.IsNullOrEmpty(request.LatitudeRaw))
        {
            address.LatitudeRaw = request.LatitudeRaw;
        }
        if (!string.IsNullOrEmpty(request.LongitudeRaw))
        {
            address.LongitudeRaw = request.LongitudeRaw;
        }

        if (!string.IsNullOrWhiteSpace(request.DeliveryInstructions))
        {
            address.DeliveryInstructions = request.DeliveryInstructions;
        }

        if (!string.IsNullOrWhiteSpace(request.BuildingType))
        {
            address.BuildingType = request.BuildingType;
        }

        if (!string.IsNullOrWhiteSpace(request.AddressLabel))
        {
            address.AddressLabel = request.AddressLabel;
        }

        if (request.BuildingDetails is not null)
        {
            address.BuildingDetails = request.BuildingDetails;
        }

        if (!string.IsNullOrWhiteSpace(request.LatitudeRaw) && !string.IsNullOrWhiteSpace(request.LongitudeRaw) && decimal.TryParse(request.LatitudeRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal lat) && decimal.TryParse(request.LongitudeRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal lng))
        {
            address.Latitude = lat;
            address.Longitude = lng;

            address.Location = Domain.Address.Address.CreatePoint(
                (double)lat,
                (double)lng);
        }
        address.UpdatedAt = dateTimeProvider.UtcNow;
        address.UpdatedBy = userId.ToString();

        context.Addresses.Update(address);
        await context.SaveChangesAsync(cancellationToken);

        return (AddressDto)address;
    }
}
