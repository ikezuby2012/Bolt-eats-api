using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Restaurant.GetCommonCategory;

internal sealed class GetCommonCategoryQueryHandler(IApplicationDbContext db) : IQueryHandler<GetCommonCategoryQuery, IReadOnlyList<CommonCategoryDto>>
{
    public async Task<Result<IReadOnlyList<CommonCategoryDto>>> Handle(GetCommonCategoryQuery query, CancellationToken cancellationToken)

    {
        int limit = query.Limit ?? 10;

        var raw = await db.Category
         .AsNoTracking()
         .Where(c => c.Restaurant.IsActive)
         .Select(c => new { c.Name, c.RestaurantId })
         .ToListAsync(cancellationToken);

        return raw
            .GroupBy(c => c.Name.Trim())
            .Select(g => new CommonCategoryDto(
                Name: g.First().Name.Trim(),
                RestaurantCount: g.DistinctBy(c => c.RestaurantId).Count(),
                IconLink: null))
            .OrderByDescending(x => x.RestaurantCount)
            .Take(limit)
            .ToList();
    }
}
