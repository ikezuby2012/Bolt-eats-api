using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Cart.Dto;
using Domain.Common;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Cart.AddCartItem;

internal sealed class AddCartItemCommandHandler(
    IApplicationDbContext db,
    IUserContext userContext,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<AddCartItemCommand, CartDto>
{
    public async Task<Result<CartDto>> Handle(
        AddCartItemCommand command,
        CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        Domain.MenuItem.MenuItem? menuItem = await db.MenuItem
            .Include(m => m.Restaurant)
            .FirstOrDefaultAsync(
                m => m.Id == command.MenuItemId && m.IsAvailable,
                cancellationToken);

        if (menuItem is null)
        {
            return Result.Failure<CartDto>(
                CommonErrors.CustomErrorMessage("Menu item not found or unavailable."));
        }


        if (!menuItem.Restaurant.IsActive || !menuItem.Restaurant.IsOpen)
        {
            return Result.Failure<CartDto>(
                CommonErrors.CustomErrorMessage("Restaurant is currently closed."));
        }


        Domain.Cart.Cart? existingCart = await db.Cart
            .Include(c => c.Restaurant)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (existingCart is not null &&
            existingCart.RestaurantId != menuItem.RestaurantId)
        {
            return Result.Failure<CartDto>(
                CommonErrors.CustomErrorMessage(
                    $"Your cart has items from {existingCart.Restaurant.Name}. " +
                    $"Adding items from {menuItem.Restaurant.Name} will clear your current cart."));
        }

        if (existingCart is null)
        {
            existingCart = new Domain.Cart.Cart
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RestaurantId = menuItem.RestaurantId,
                PromoCode = command.PromoCode,
                CreatedBy = userId.ToString(),
                CreatedAt = dateTimeProvider.UtcNow,
                Items = []
            };
            await db.Cart.AddAsync(existingCart, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }

        Domain.Cart.CartItem? existingLine = await db.CartItems
            .FirstOrDefaultAsync(
                i => i.CartId == existingCart.Id &&
                     i.MenuItemId == command.MenuItemId,
                cancellationToken);

        if (existingLine is not null)
        {
            int newQty = existingLine.Quantity + command.Quantity;

            if (newQty > 20)
            {
                return Result.Failure<CartDto>(
                   CommonErrors.CustomErrorMessage(
                       $"Maximum quantity of 20 per item exceeded. " +
                       $"You already have {existingLine.Quantity} in your cart."));
            }

            await db.CartItems
                .Where(i => i.Id == existingLine.Id)
                .ExecuteUpdateAsync(
                    s => s.SetProperty(i => i.Quantity, newQty),
                    cancellationToken);
        }
        else
        {
            var newItem = new Domain.Cart.CartItem
            {
                Id = Guid.NewGuid(),
                CartId = existingCart.Id,
                MenuItemId = menuItem.Id,
                Quantity = command.Quantity,
                UnitPrice = menuItem.DiscountPrice ?? menuItem.Price,
                Notes = command.Notes,
                CreatedBy = userId.ToString(),
                CreatedAt = dateTimeProvider.UtcNow,
            };

            await db.CartItems.AddAsync(newItem, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }

        Domain.Cart.Cart cart = await db.Cart
            .AsNoTracking()
            .Include(c => c.Items)
                .ThenInclude(i => i.MenuItem)
            .Include(c => c.Restaurant)
            .FirstAsync(c => c.Id == existingCart.Id, cancellationToken);

        return (CartDto)cart;
    }
}
