using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Cart.Dto;
using Application.Cart.GetUserCart;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Cart.GetCart;

internal sealed class GetUserCartQueryHandler(IApplicationDbContext context, IUserContext userContext) : IQueryHandler<GetUserCartQuery, PaginatedResult<CartDto>>
{
    public async Task<Result<PaginatedResult<CartDto>>> Handle(GetUserCartQuery query, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        IQueryable<Domain.Cart.Cart> baseQuery = context.Cart.AsNoTracking().AsQueryable().Include(c => c.Items).ThenInclude(i => i.MenuItem).Where(x => x.UserId == userId);

        int totalCounts = await baseQuery.CountAsync(cancellationToken);

        List<CartDto> allUserCarts = await baseQuery.OrderByDescending(m => m.CreatedAt)
            .Skip((query.pageNumber - 1) * query.PageSize)
            .Select(x => (CartDto)x).ToListAsync(cancellationToken);

        return new PaginatedResult<CartDto>
        {
            Data = allUserCarts,
            TotalItems = totalCounts,
            PageSize = query.PageSize,
            PageNumber = query.pageNumber,
        };
    }
}
