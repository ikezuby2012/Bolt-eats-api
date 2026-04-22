using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Cart.Dto;
using Domain.MenuItem;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using SharedKernel;

namespace Application.Cart.UpdateCartItem;

internal sealed class UpdateCartItemCommandHandler(IApplicationDbContext context, IUserContext userContext) : ICommandHandler<UpdateCartItemCommand, CartDto>
{
    public async Task<Result<CartDto>> Handle(UpdateCartItemCommand command, CancellationToken cancellationToken)
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
            return Result.Failure<CartDto>(Domain.Common.CommonErrors.CustomErrorMessage("No Active cart"));
        }

        Domain.Cart.CartItem? item = cart.Items.FirstOrDefault(i => i.Id == command.ItemId);

        if (item is null)
        {
            return Result.Failure<CartDto>(Domain.Common.CommonErrors.CustomErrorMessage("Cart Item not found"));
        }

        item.Quantity = command.Quantity;

        await context.SaveChangesAsync(cancellationToken);

        return (CartDto)cart;
    }
}
