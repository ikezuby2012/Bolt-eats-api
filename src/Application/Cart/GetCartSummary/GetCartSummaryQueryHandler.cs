using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Domain.Cart;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Cart.GetCartSummary;

internal sealed class GetCartSummaryQueryHandler(IApplicationDbContext context, IUserContext userContext, ICartService cartService) : IQueryHandler<GetCartSummaryQuery, CartSummaryDto>
{
    public async Task<Result<CartSummaryDto>> Handle(GetCartSummaryQuery query, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        Domain.Cart.Cart? cart = await context.Cart
            .Include(c => c.Restaurant)
            .Include(c => c.Items)
                .ThenInclude(i => i.MenuItem)
            .FirstOrDefaultAsync(
                c => c.UserId == userId,
                cancellationToken);

        if (cart is null)
        {
            return Result.Failure<CartSummaryDto>(Domain.Common.CommonErrors.CustomErrorMessage("No Active cart"));
        }

        CartSummaryDto summary = cartService.Calculate(cart);

        return summary;
    }
}
