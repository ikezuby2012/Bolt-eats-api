using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;

namespace Application.Restaurant.GetCheapDelivery;

public sealed record GetCheapDeliveryQuery(decimal MaxDeliveryFee = 200, int Limit = 10) : IQuery<IReadOnlyList<HomeMenuItemDto>>;
