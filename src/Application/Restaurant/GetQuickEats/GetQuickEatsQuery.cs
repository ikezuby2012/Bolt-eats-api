using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;

namespace Application.Restaurant.GetQuickEats;
public sealed record GetQuickEatsQuery(int limit) : IQuery<IReadOnlyList<HomeMenuItemDto>>;
