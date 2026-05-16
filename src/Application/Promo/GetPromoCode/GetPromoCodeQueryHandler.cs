using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Promo.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Promo.GetPromoCode;

internal sealed class GetPromoCodeQueryHandler(IApplicationDbContext db, IDateTimeProvider dateTimeProvider) : IQueryHandler<GetPromoCodeQuery, PaginatedResult<PromoCodeDto>>
{
    public async Task<Result<PaginatedResult<PromoCodeDto>>> Handle(GetPromoCodeQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Domain.PromoCode.PromoCode> query = db.PromoCode.AsNoTracking()
            .Include(p => p.Restaurant)
            .AsQueryable();

        if (request.ActiveOnly == true)
        {
            query = query.Where(p => p.IsActive && p.ExpiresAt > dateTimeProvider.UtcNow);
        }
        if (request.ActiveOnly == false)
        {
            query = query.Where(p => !p.IsActive || p.ExpiresAt <= DateTime.UtcNow);
        }
        if (request.RestaurantId.HasValue)
        {
            query = query.Where(p => p.RestaurantId == request.RestaurantId);
        }

        int total = await query.CountAsync(cancellationToken);

        List<PromoCodeDto> items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => (PromoCodeDto)p)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<PromoCodeDto>
        {
            Data = items,
            TotalItems = total,
            PageNumber = request.Page,
            PageSize = request.PageSize
        };
    }
}
