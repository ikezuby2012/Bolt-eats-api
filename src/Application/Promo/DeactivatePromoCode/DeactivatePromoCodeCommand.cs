using Application.Abstractions.Messaging;

namespace Application.Promo.DeactivatePromoCode;

public sealed record DeactivatePromoCodeCommand(Guid Id) : ICommand;
