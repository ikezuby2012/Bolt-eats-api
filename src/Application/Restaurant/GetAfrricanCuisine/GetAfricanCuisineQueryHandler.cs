using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using Domain.MenuItem;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Restaurant.GetAfrricanCuisine;

internal sealed class GetAfricanCuisineQueryHandler(IApplicationDbContext db) : IQueryHandler<GetAfricanCuisineQuery, IReadOnlyList<AfricanCuisineItemDto>>
{
    public async Task<Result<IReadOnlyList<AfricanCuisineItemDto>>> Handle(GetAfricanCuisineQuery request, CancellationToken cancellationToken)
    {
        IQueryable<MenuItem> query = db.MenuItem
         .AsNoTracking()
         .Where(m =>
             m.IsAvailable &&
             m.Restaurant.IsActive &&
             m.Restaurant.IsOpen);

        query = query.Where(m =>
            EF.Functions.Like(m.Category.Name, "%Soup%") ||
            EF.Functions.Like(m.Category.Name, "%Swallow%") ||
            EF.Functions.Like(m.Category.Name, "%Seafood%") ||
            EF.Functions.Like(m.Category.Name, "%Nigerian Cuisine%") ||
            EF.Functions.Like(m.Name, "%Soup%") ||
            EF.Functions.Like(m.Name, "%Swallow%") ||
            EF.Functions.Like(m.Name, "%Seafood%"));

        return await query
           .OrderByDescending(m => m.IsPopular)
           .ThenBy(m => m.Price)
           .Take(request.Limit)
           .Select(m => new AfricanCuisineItemDto(
               m.Id,
               m.Name,
               m.Price,
               m.DiscountPrice,
               m.ImageUrl,
               m.Category.Name,
               m.RestaurantId,
               m.Restaurant.Name,
               m.Restaurant.LogoUrl))
           .ToListAsync(cancellationToken);
    }
}
