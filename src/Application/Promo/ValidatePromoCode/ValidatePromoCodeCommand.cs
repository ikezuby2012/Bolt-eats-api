using Application.Abstractions.Messaging;
using Application.Promo.Dto;

namespace Application.Promo.ValidatePromoCode;

public sealed record ValidatePromoCodeCommand(string code) : ICommand<PromoValidationResultDto>;
