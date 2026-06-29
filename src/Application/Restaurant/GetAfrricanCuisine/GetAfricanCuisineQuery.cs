using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;

namespace Application.Restaurant.GetAfrricanCuisine;

public sealed record GetAfricanCuisineQuery(int Limit) : IQuery<IReadOnlyList<AfricanCuisineItemDto>>;
