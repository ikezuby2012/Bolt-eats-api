using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;

namespace Application.Restaurant.GetInternationalBites;

public sealed record GetInternationalBitesQuery(int Limit = 10)
    : IQuery<IReadOnlyList<HomeSectionItemDto>>;
