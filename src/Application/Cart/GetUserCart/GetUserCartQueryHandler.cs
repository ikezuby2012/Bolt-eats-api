using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Cart.Dto;
using Application.Cart.GetUserCart;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Cart.GetCart;

internal sealed class GetUserCartQueryHandler(
    IApplicationDbContext context,
    IUserContext userContext) : IQueryHandler<GetUserCartQuery, CartDto>
{
    public async Task<Result<CartDto>> Handle(
        GetUserCartQuery query,
        CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        Domain.Cart.Cart? cart = await context.Cart
            .AsNoTracking()
            .Include(c => c.Restaurant)
            .Include(c => c.Items)
                .ThenInclude(i => i.MenuItem)
                    .ThenInclude(m => m.Category)
            .Include(c => c.Items)
                .ThenInclude(i => i.MenuItem)
                    .ThenInclude(m => m.Restaurant)
            .FirstOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken);

        if (cart == null)
        {
            return Result.Success(new CartDto());
        }

        return (CartDto)cart;
    }
}
