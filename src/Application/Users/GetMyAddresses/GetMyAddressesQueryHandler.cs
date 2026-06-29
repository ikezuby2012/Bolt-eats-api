using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Users.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.GetMyAddresses;

internal sealed class GetMyAddressesQueryHandler(IUserContext userContext, IApplicationDbContext context) : IQueryHandler<GetMyAddressesQuery, IEnumerable<AddressDto>>
{
    public async Task<Result<IEnumerable<AddressDto>>> Handle(GetMyAddressesQuery query, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        List<AddressDto> userAddress = await context.Addresses.Where(a => a.UserId == userId)
            .Select(address => new AddressDto
            {
                Id = address.Id,
                UserId = address.UserId,
                Label = address.Label,
                Street = address.Street,
                City = address.City,
                State = address.State,
                Country = address.Country,
                PostalCode = address.PostalCode,
                Latitude = address.Latitude,
                Longitude = address.Longitude,
                LatitudeRaw = address.LatitudeRaw,
                LongitudeRaw = address.LongitudeRaw,
                IsDefault = address.IsDefault,
                CreatedAt = address.CreatedAt,
                UpdatedAt = address.UpdatedAt,
                DeliveryInstructions = address.DeliveryInstructions,
                BuildingType = address.BuildingType,
                AddressLabel = address.AddressLabel,
                BuildingDetails = address.BuildingDetails,
            }).ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<AddressDto>>(userAddress);
    }
}
