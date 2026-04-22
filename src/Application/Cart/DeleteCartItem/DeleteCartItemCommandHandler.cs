using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Cart.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Cart.DeleteCartItem;

internal sealed class DeleteCartItemCommandHandler(IApplicationDbContext context, IUserContext userContext) : ICommandHandler<DeleteCartItemCommand, CartDto>
{
    public async Task<Result<CartDto>> Handle(DeleteCartItemCommand command, CancellationToken cancellationToken)
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

        item.IsSoftDeleted = true;
        context.CartItems.Update(item);

        if (cart.Items.Count == 1)
        {
            cart.PromoCode = null;
        }

        await context.SaveChangesAsync(cancellationToken);

        return (CartDto)cart;
    }
}
