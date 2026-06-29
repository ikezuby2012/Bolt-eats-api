using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;

namespace Application.Restaurant.GetProtein;

public sealed record GetProteinFixQuery(int Limit = 10)
    : IQuery<IReadOnlyList<HomeSectionItemDto>>;
