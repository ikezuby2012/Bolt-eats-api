using Application.Abstractions.Messaging;
using Application.Users.Dto;

namespace Application.Users.GetMyAddresses;

public sealed record GetMyAddressesQuery : IQuery<IEnumerable<AddressDto>>;
