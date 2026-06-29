using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;

namespace Application.Restaurant.GetRecentOffer;

public sealed record GetRecentOfferQuery(int Limit) : IQuery<IReadOnlyList<HomeMenuItemDto>>;
