using Application.Abstractions.Messaging;
using Application.Users.Dto;

namespace Application.Users.UpdateMyAddress;

public sealed record UpdateMyAddressCommand(
    Guid Id,
    string Label,
    string Street,
    string City,
    string State,
    string Country,
    string PostalCode,
    string LatitudeRaw,
    string LongitudeRaw,
    bool IsDefault
) : ICommand<AddressDto>;
