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

        if (!string.IsNullOrEmpty(request.LatitudeRaw) && decimal.TryParse(request.LatitudeRaw, out decimal lat))
        {
            address.Latitude = lat;
        }

        if (!string.IsNullOrEmpty(request.LongitudeRaw) && decimal.TryParse(request.LongitudeRaw, out decimal lng))
        {
            address.Longitude = lng;
        }
        address.UpdatedAt = dateTimeProvider.UtcNow;
        address.UpdatedBy = userId.ToString();

        context.Addresses.Update(address);
        await context.SaveChangesAsync(cancellationToken);

        return (AddressDto)address;
    }
}
