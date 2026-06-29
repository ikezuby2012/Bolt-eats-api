using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;

namespace Application.Restaurant.GetCommonCategory;

public sealed record GetCommonCategoryQuery(int? Limit = 10) : IQuery<IReadOnlyList<CommonCategoryDto>>;
