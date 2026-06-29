using Application.Abstractions.Messaging;
using Application.Users.Dto;

namespace Application.Users.CreateNewAddress;


public sealed record CreateNewAddressCommand(
    string Label,
    string Street,
    string City,
    string State,
    string Country,
    string PostalCode,
    string LatitudeRaw,
    string LongitudeRaw,
    string? DeliveryInstructions,
    string? BuildingType,
    string? AddressLabel,
    Dictionary<string, string>? BuildingDetails,
    bool IsDefault
) : ICommand<AddressDto>;
