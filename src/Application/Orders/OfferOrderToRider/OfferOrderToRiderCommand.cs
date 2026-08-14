using Application.Abstractions.Messaging;

namespace Application.Orders.OfferOrderToRider;

public sealed record OfferOrderToRiderCommand(
    Guid OrderId,
    Guid RiderId)
    : ICommand;
