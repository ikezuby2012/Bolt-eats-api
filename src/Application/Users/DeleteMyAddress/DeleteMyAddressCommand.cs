
using Application.Abstractions.Messaging;

namespace Application.Users.DeleteMyAddress;

public sealed record DeleteMyAddressCommand(Guid Id) : ICommand;
