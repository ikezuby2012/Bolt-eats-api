using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Cart.Dto;
using Domain.Common;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Restaurant.UpdateCartItemQuantity;

internal sealed class UpdateCartItemQuantityCommandHandler(
    IApplicationDbContext db,
    IUserContext userContext)
    : ICommandHandler<UpdateCartItemQuantityCommand, CartDto>
{
    public async Task<Result<CartDto>> Handle(
        UpdateCartItemQuantityCommand command,
        CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        Domain.Cart.CartItem? cartItem = await db.CartItems
            .Include(i => i.Cart)
            .FirstOrDefaultAsync(
                i => i.Id == command.CartItemId &&
                     i.Cart.UserId == userId,
                cancellationToken);

        if (cartItem is null)
        {
            return Result.Failure<CartDto>(
               Error.NotFound("CartItem.NotFound", "Cart item not found."));
        }

        switch (command.Action)
        {
            case QuantityAction.Increase:

                if (cartItem.Quantity >= 20)
                {
                    return Result.Failure<CartDto>(
                        CommonErrors.CustomErrorMessage(
                            "Maximum quantity of 20 per item reached."));
                }


                await db.CartItems
                    .Where(i => i.Id == command.CartItemId)
                    .ExecuteUpdateAsync(
                        s => s.SetProperty(i => i.Quantity, cartItem.Quantity + 1),
                        cancellationToken);
                break;

            case QuantityAction.Decrease:

                if (cartItem.Quantity == 1)
                {
                    await db.CartItems
                        .Where(i => i.Id == command.CartItemId)
                        .ExecuteUpdateAsync(
                            s => s.SetProperty(i => i.IsSoftDeleted, true),
                            cancellationToken);
                }
                else
                {
                    await db.CartItems
                        .Where(i => i.Id == command.CartItemId)
                        .ExecuteUpdateAsync(
                            s => s.SetProperty(i => i.Quantity, cartItem.Quantity - 1),
                            cancellationToken);
                }
                break;
        }

        Domain.Cart.Cart cart = await db.Cart
            .AsNoTracking()
            .Include(c => c.Items.Where(i => !i.IsSoftDeleted))
                .ThenInclude(i => i.MenuItem)
            .Include(c => c.Restaurant)
            .FirstAsync(c => c.Id == cartItem.CartId, cancellationToken);

        return (CartDto)cart;
    }
}
