using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;

namespace Application.Restaurant.GetDrinksAndSmoothies;

public sealed record GetDrinksAndSmoothiesQuery(int Limit = 10)
    : IQuery<IReadOnlyList<HomeSectionItemDto>>;
