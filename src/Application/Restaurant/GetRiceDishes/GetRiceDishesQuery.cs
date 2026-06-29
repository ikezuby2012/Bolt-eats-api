using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;

namespace Application.Restaurant.GetRiceDishes;

public sealed record GetRiceDishesQuery(int Limit = 10)
    : IQuery<IReadOnlyList<HomeSectionItemDto>>;
