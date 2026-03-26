using Application.Abstractions.Messaging;
using Application.Users.Dto;

namespace Application.Users.SetAddressAsDefault;

public sealed record SetAddressAsDefaultCommand(Guid Id) : ICommand<AddressDto>;
