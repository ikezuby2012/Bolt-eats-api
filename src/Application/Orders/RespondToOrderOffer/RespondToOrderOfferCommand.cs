using Application.Abstractions.Messaging;

namespace Application.Orders.RespondToOrderOffer;

public sealed record RespondToOrderOfferCommand(
    Guid OrderId,
    Guid RiderId,
    bool Accepted)
    : ICommand;
