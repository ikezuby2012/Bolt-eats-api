using Application.Abstractions.Messaging;

namespace Application.Cart.ClearCart;

public sealed record ClearCartCommand(Guid CartId) : ICommand;
